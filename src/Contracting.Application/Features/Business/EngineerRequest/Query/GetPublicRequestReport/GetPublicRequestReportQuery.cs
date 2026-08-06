using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;
using MasterEngineer = Contracting.Domain.Entities.master.Engineer;
using MasterStatus = Contracting.Domain.Entities.master.Status;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetPublicRequestReport;

// Anonymous read used by the public printable report page. No visibility filter —
// access control is the unguessable request GUID (same posture as the public site-report endpoints).
public record GetPublicRequestReportQuery(Guid RequestId) : IRequest<ErrorOr<GetPublicEngineerRequestReportDto>>;

public class GetPublicRequestReportQueryHandler : IRequestHandler<GetPublicRequestReportQuery, ErrorOr<GetPublicEngineerRequestReportDto>>
{
    private static readonly string[] CompletedKeywords = { "completed", "complete", "done", "finished", "finish", "closed" };
    private static readonly string[] RejectedKeywords = { "rejected", "reject", "denied", "deny" };

    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly IStorageService _s3;

    public GetPublicRequestReportQueryHandler(ApplicationDbContext db, IMapper mapper, IStorageService s3)
    {
        _db = db;
        _mapper = mapper;
        _s3 = s3;
    }

    public async Task<ErrorOr<GetPublicEngineerRequestReportDto>> Handle(GetPublicRequestReportQuery query, CancellationToken cancellationToken)
    {
        var request = await _db.EngineerRequests
            .AsNoTracking()
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.Engineer)
                .ThenInclude(e => e.Department)
            .Include(r => r.assignTo)
                .ThenInclude(e => e.Department)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.Engineer)
            .Include(r => r.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Engineer)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Status)
            .Include(r => r.SpecialFieldValues)
                .ThenInclude(v => v.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .Include(r => r.SpecialFieldItems)
                .ThenInclude(i => i.ConstructionItem)
                    .ThenInclude(c => c.Units)
            .Include(r => r.SpecialFieldListItems)
                .ThenInclude(i => i.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .Include(r => r.PurchaseReceipts)
                .ThenInclude(rc => rc.ReceivedBy)
            .Where(r => r.Id == query.RequestId)
            .AsSplitQuery()
            .FirstOrDefaultAsync(cancellationToken);

        if (request is null)
            return Error.NotFound("EngineerRequest.NotFound", "Engineer request not found.");

        var dto = _mapper.Map<GetAllEngineerRequestDto>(request);

        dto.Receipts = request.PurchaseReceipts?
            .OrderByDescending(rc => rc.ReceiptDate)
            .Select(rc => new GetGoodsReceiptDto
            {
                Id = rc.Id,
                ReceiptDate = rc.ReceiptDate,
                IsPartialReceipt = rc.IsPartialReceipt,
                IsConfirmed = rc.IsConfirmed,
                Notes = rc.Notes,
                ReceivedById = rc.ReceivedById,
                ReceivedBy = rc.ReceivedBy is null ? null : _mapper.Map<GetEngineerDto>(rc.ReceivedBy)
            })
            .ToList() ?? new();

        var report = new GetPublicEngineerRequestReportDto
        {
            Request = dto,
            CreatedDate = request.CreatedDate,
            Creator = BuildSignatory(request.Engineer, request.CreatedDate, "Created")
        };

        // The approval/rejection decision lives in the activity trail: the most recent
        // activity whose status matches a terminal keyword identifies the deciding engineer.
        var decisionActivity = request.EngineerRequestActivites?
            .Where(a => a.Status != null && a.Engineer != null)
            .OrderByDescending(a => a.CreatedDate)
            .FirstOrDefault(a =>
                StatusMatches(a.Status!, RejectedKeywords) ||
                StatusMatches(a.Status!, CompletedKeywords) ||
                a.ActionType == EngineerRequestActionType.ClosedOnReceipt.ToString());

        if (decisionActivity is not null)
        {
            var decision = StatusMatches(decisionActivity.Status!, RejectedKeywords) ? "Rejected" : "Approved";
            report.Approver = BuildSignatory(decisionActivity.Engineer, decisionActivity.CreatedDate, decision);
        }
        else if (request.Status != null &&
                 (StatusMatches(request.Status, RejectedKeywords) || StatusMatches(request.Status, CompletedKeywords)) &&
                 request.assignTo != null)
        {
            // Legacy data: terminal status but no matching activity row — fall back to the assignee.
            var decision = StatusMatches(request.Status, RejectedKeywords) ? "Rejected" : "Approved";
            report.Approver = BuildSignatory(request.assignTo, request.ModifiedDate, decision);
        }

        await AttachSignatureUrlsAsync(report, cancellationToken);
        return report;
    }

    private static bool StatusMatches(MasterStatus status, string[] keywords)
        => keywords.Any(k =>
            (status.Code?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameEn?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameAr?.Contains(k, StringComparison.OrdinalIgnoreCase) == true));

    private static RequestSignatoryDto? BuildSignatory(MasterEngineer? engineer, DateTimeOffset? signedDate, string decision)
        => engineer is null ? null : new RequestSignatoryDto
        {
            EngineerId = engineer.Id,
            NameEn = engineer.nameEn,
            NameAr = engineer.nameAr,
            Position = engineer.position,
            SignedDate = signedDate,
            Decision = decision
        };

    private async Task AttachSignatureUrlsAsync(GetPublicEngineerRequestReportDto report, CancellationToken cancellationToken)
    {
        var engineerIds = new[] { report.Creator?.EngineerId, report.Approver?.EngineerId }
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToList();

        if (engineerIds.Count == 0) return;

        var signatures = await (
                from e in _db.Engineers
                join s in _db.UserSignatures on e.ApplicationUserId equals s.UserId
                where engineerIds.Contains(e.Id)
                select new { EngineerId = e.Id, s.SignatureUrl })
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        foreach (var signatory in new[] { report.Creator, report.Approver })
        {
            if (signatory is null) continue;
            var key = signatures.FirstOrDefault(s => s.EngineerId == signatory.EngineerId)?.SignatureUrl;
            if (!string.IsNullOrWhiteSpace(key))
                signatory.SignatureUrl = _s3.GetPreSignedUrl(key);
        }
    }
}
