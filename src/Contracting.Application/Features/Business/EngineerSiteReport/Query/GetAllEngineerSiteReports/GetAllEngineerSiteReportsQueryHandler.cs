using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetAllEngineerSiteReports
{
    public class GetAllEngineerSiteReportsQueryHandler : IRequestHandler<GetAllEngineerSiteReportsQuery, ErrorOr<PaginatedList<GetEngineerSiteReportDto>>>
    {
        private readonly IEngineerSiteReportService _service;

        public GetAllEngineerSiteReportsQueryHandler(IEngineerSiteReportService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetEngineerSiteReportDto>>> Handle(GetAllEngineerSiteReportsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllEngineerSiteReportsAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
