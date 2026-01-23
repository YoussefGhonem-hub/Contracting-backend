using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportsByEngineerId
{
    public class GetEngineerSiteReportsByEngineerIdQueryHandler : IRequestHandler<GetEngineerSiteReportsByEngineerIdQuery, ErrorOr<PaginatedList<GetEngineerSiteReportDto>>>
    {
        private readonly IEngineerSiteReportService _service;

        public GetEngineerSiteReportsByEngineerIdQueryHandler(IEngineerSiteReportService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetEngineerSiteReportDto>>> Handle(GetEngineerSiteReportsByEngineerIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerSiteReportsByEngineerIdAsync(request.EngineerId, request.Filter, cancellationToken);
            return result;
        }
    }
}
