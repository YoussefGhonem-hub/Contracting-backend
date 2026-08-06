using Contracting.Application.Features.Business.Common;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using ErrorOr;
using MediatR;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Business.FinancialClearance.Query.GetPublicFinancialClearanceReport;

// Anonymous read used by the public printable report page. Access control is
// the unguessable request GUID (same posture as the other public report endpoints).
public record GetPublicFinancialClearanceReportQuery(Guid Id) : IRequest<ErrorOr<PublicReportDto<GetFinancialClearanceDto>>>;

public class GetPublicFinancialClearanceReportQueryHandler
    : IRequestHandler<GetPublicFinancialClearanceReportQuery, ErrorOr<PublicReportDto<GetFinancialClearanceDto>>>
{
    private readonly IFinancialClearanceService _service;
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _s3;

    public GetPublicFinancialClearanceReportQueryHandler(IFinancialClearanceService service, ApplicationDbContext db, IStorageService s3)
    {
        _service = service;
        _db = db;
        _s3 = s3;
    }

    public async Task<ErrorOr<PublicReportDto<GetFinancialClearanceDto>>> Handle(
        GetPublicFinancialClearanceReportQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdForPublicReportAsync(query.Id);
        if (result.IsError) return result.Errors;

        var dto = result.Value;
        var report = new PublicReportDto<GetFinancialClearanceDto>
        {
            Request = dto,
            CreatedDate = dto.CreatedDate,
            Creator = PublicReportHelpers.BuildSignatory(dto.RequestedBy, dto.CreatedDate, "Created")
        };

        // Both "approve" and "close" land on COMPLETED — prefer the actual
        // approve action so the report shows who approved, not who archived.
        var activities = (dto.Activities ?? new())
            .Where(a => a.Engineer != null)
            .OrderByDescending(a => a.CreatedDate)
            .ToList();

        var rejectActivity = activities.FirstOrDefault(a =>
            PublicReportHelpers.StatusMatches(a.ToStatus, PublicReportHelpers.RejectedKeywords));
        var approveActivity = activities.FirstOrDefault(a =>
                string.Equals(a.ActionType?.Trim(), "approve", StringComparison.OrdinalIgnoreCase))
            ?? activities.FirstOrDefault(a =>
                PublicReportHelpers.StatusMatches(a.ToStatus, PublicReportHelpers.CompletedKeywords));

        if (rejectActivity is not null)
        {
            report.Approver = PublicReportHelpers.BuildSignatory(rejectActivity.Engineer, rejectActivity.CreatedDate, "Rejected");
        }
        else if (approveActivity is not null)
        {
            report.Approver = PublicReportHelpers.BuildSignatory(approveActivity.Engineer, approveActivity.CreatedDate, "Approved");
        }

        await PublicReportHelpers.AttachSignatureUrlsAsync(_db, _s3, new[] { report.Creator, report.Approver }, cancellationToken);
        return report;
    }
}
