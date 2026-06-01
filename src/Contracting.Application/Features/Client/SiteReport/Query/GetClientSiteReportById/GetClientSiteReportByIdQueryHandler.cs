using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ReportDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.SiteReport.Query.GetClientSiteReportById;

public class GetClientSiteReportByIdQueryHandler : IRequestHandler<GetClientSiteReportByIdQuery, ErrorOr<GetClientSiteReportDetailDto>>
{
    private readonly IClientSiteReportService _service;

    public GetClientSiteReportByIdQueryHandler(IClientSiteReportService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientSiteReportDetailDto>> Handle(GetClientSiteReportByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetClientSiteReportByIdAsync(request.ReportId, cancellationToken);
        if (result is null)
            return Error.NotFound(description: "Report not found.");
        return result;
    }
}
