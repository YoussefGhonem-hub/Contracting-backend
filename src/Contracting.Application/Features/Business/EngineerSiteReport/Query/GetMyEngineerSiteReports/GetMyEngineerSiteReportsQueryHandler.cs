using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetMyEngineerSiteReports
{
    public class GetMyEngineerSiteReportsQueryHandler : IRequestHandler<GetMyEngineerSiteReportsQuery, ErrorOr<PaginatedList<GetEngineerSiteReportDto>>>
    {
        private readonly IEngineerSiteReportService _service;

        public GetMyEngineerSiteReportsQueryHandler(IEngineerSiteReportService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetEngineerSiteReportDto>>> Handle(GetMyEngineerSiteReportsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetMyEngineerSiteReportsAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
