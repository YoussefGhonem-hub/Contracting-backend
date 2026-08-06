using Contracting.Application.Features.Business.Common;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Business.TransferRequest.Query.GetPublicTransferRequestReport;

// Anonymous read used by the public printable report page. Access control is
// the unguessable request GUID (same posture as the other public report endpoints).
// TransferRequestService.GetByIdAsync has no CurrentUser dependency, so it is
// reused directly.
public record GetPublicTransferRequestReportQuery(Guid Id) : IRequest<ErrorOr<PublicReportDto<GetTransferRequestDto>>>;

public class GetPublicTransferRequestReportQueryHandler
    : IRequestHandler<GetPublicTransferRequestReportQuery, ErrorOr<PublicReportDto<GetTransferRequestDto>>>
{
    private readonly ITransferRequestService _service;
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _s3;

    public GetPublicTransferRequestReportQueryHandler(ITransferRequestService service, ApplicationDbContext db, IStorageService s3)
    {
        _service = service;
        _db = db;
        _s3 = s3;
    }

    public async Task<ErrorOr<PublicReportDto<GetTransferRequestDto>>> Handle(
        GetPublicTransferRequestReportQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdAsync(query.Id);
        if (result.IsError) return result.Errors;

        var dto = result.Value;
        var report = new PublicReportDto<GetTransferRequestDto>
        {
            Request = dto,
            CreatedDate = dto.CreatedDate,
            Creator = PublicReportHelpers.BuildSignatory(dto.RequestedBy, dto.CreatedDate, "Created")
        };

        // Transfer decisions: confirm receipt → Completed ("Received"),
        // cancel → Rejected ("Cancelled").
        var decisionActivity = dto.Activities?
            .Where(a => a.Engineer != null)
            .OrderByDescending(a => a.CreatedDate)
            .FirstOrDefault(a =>
                PublicReportHelpers.StatusMatches(a.ToStatus, PublicReportHelpers.RejectedKeywords) ||
                PublicReportHelpers.StatusMatches(a.ToStatus, PublicReportHelpers.CompletedKeywords));

        if (decisionActivity is not null)
        {
            var decision = PublicReportHelpers.StatusMatches(decisionActivity.ToStatus, PublicReportHelpers.RejectedKeywords)
                ? "Cancelled"
                : "Received";
            report.Approver = PublicReportHelpers.BuildSignatory(decisionActivity.Engineer, decisionActivity.CreatedDate, decision);
        }

        await PublicReportHelpers.AttachSignatureUrlsAsync(_db, _s3, new[] { report.Creator, report.Approver }, cancellationToken);
        return report;
    }
}
