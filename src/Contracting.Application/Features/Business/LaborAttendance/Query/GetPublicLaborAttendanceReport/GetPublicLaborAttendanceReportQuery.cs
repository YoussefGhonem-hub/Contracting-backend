using Contracting.Application.Features.Business.Common;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Business.LaborAttendance.Query.GetPublicLaborAttendanceReport;

// Anonymous read used by the public printable report page. Access control is
// the unguessable request GUID (same posture as the other public report endpoints).
public record GetPublicLaborAttendanceReportQuery(Guid Id) : IRequest<ErrorOr<PublicReportDto<GetLaborAttendanceRequestDto>>>;

public class GetPublicLaborAttendanceReportQueryHandler
    : IRequestHandler<GetPublicLaborAttendanceReportQuery, ErrorOr<PublicReportDto<GetLaborAttendanceRequestDto>>>
{
    private readonly ILaborAttendanceService _service;
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _s3;

    public GetPublicLaborAttendanceReportQueryHandler(ILaborAttendanceService service, ApplicationDbContext db, IStorageService s3)
    {
        _service = service;
        _db = db;
        _s3 = s3;
    }

    public async Task<ErrorOr<PublicReportDto<GetLaborAttendanceRequestDto>>> Handle(
        GetPublicLaborAttendanceReportQuery query, CancellationToken cancellationToken)
    {
        var result = await _service.GetByIdForPublicReportAsync(query.Id);
        if (result.IsError) return result.Errors;

        var dto = result.Value;
        var report = new PublicReportDto<GetLaborAttendanceRequestDto>
        {
            Request = dto,
            CreatedDate = dto.CreatedDate,
            Creator = PublicReportHelpers.BuildSignatory(dto.Supervisor, dto.CreatedDate, "Created")
        };

        // The validate/reject decision lives in the activity trail: the most
        // recent activity whose ToStatus is terminal identifies the decider.
        var decisionActivity = dto.Activities?
            .Where(a => a.Engineer != null)
            .OrderByDescending(a => a.CreatedDate)
            .FirstOrDefault(a =>
                PublicReportHelpers.StatusMatches(a.ToStatus, PublicReportHelpers.RejectedKeywords) ||
                PublicReportHelpers.StatusMatches(a.ToStatus, PublicReportHelpers.CompletedKeywords));

        if (decisionActivity is not null)
        {
            var decision = PublicReportHelpers.StatusMatches(decisionActivity.ToStatus, PublicReportHelpers.RejectedKeywords)
                ? "Rejected"
                : "Approved";
            report.Approver = PublicReportHelpers.BuildSignatory(decisionActivity.Engineer, decisionActivity.CreatedDate, decision);
        }

        await PublicReportHelpers.AttachSignatureUrlsAsync(_db, _s3, new[] { report.Creator, report.Approver }, cancellationToken);
        return report;
    }
}
