using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetOverdueRisk
{
    public class GetOverdueRiskQueryHandler : IRequestHandler<GetOverdueRiskQuery, ErrorOr<OverdueRiskDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetOverdueRiskQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<OverdueRiskDto>> Handle(GetOverdueRiskQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetOverdueRiskAsync(cancellationToken);
            return result;
        }
    }
}
