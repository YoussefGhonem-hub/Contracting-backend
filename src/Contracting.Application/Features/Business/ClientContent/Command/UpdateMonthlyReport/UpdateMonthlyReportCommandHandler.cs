using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.ClientContent.Command.UpdateMonthlyReport;

public class UpdateMonthlyReportCommandHandler : IRequestHandler<UpdateMonthlyReportCommand, ErrorOr<UpdatedMonthlyReportDto>>
{
    private readonly IClientContentService _service;

    public UpdateMonthlyReportCommandHandler(IClientContentService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<UpdatedMonthlyReportDto>> Handle(UpdateMonthlyReportCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.UpdateMonthlyReportAsync(
            request.ReportId, request.Month, request.Year, request.Title, request.WorkProgress,
            request.Attachments, request.RemoveAttachmentIds, cancellationToken);

        if (result is null)
            return Error.NotFound("MonthlyReport.NotFound", "Report not found.");

        return result;
    }
}
