using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAgingReport
{
    public class GetAgingReportQueryHandler : IRequestHandler<GetAgingReportQuery, ErrorOr<AgingReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetAgingReportQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<AgingReportDto>> Handle(GetAgingReportQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAgingReportAsync(cancellationToken);
            return result;
        }
    }
}
