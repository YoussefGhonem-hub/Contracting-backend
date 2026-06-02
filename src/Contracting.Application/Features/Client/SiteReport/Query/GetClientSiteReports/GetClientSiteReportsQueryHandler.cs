using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ReportDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.SiteReport.Query.GetClientSiteReports;

public class GetClientSiteReportsQueryHandler : IRequestHandler<GetClientSiteReportsQuery, ErrorOr<List<GetClientSiteReportListItemDto>>>
{
    private readonly IClientSiteReportService _service;

    public GetClientSiteReportsQueryHandler(IClientSiteReportService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<List<GetClientSiteReportListItemDto>>> Handle(GetClientSiteReportsQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetClientSiteReportsAsync(request.ProjectId, cancellationToken: cancellationToken);
        if (result is null)
            return Error.NotFound(description: "Project not found or you do not have access.");
        return result;
    }
}
