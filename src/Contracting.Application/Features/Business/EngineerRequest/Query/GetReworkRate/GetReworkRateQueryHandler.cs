using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetReworkRate
{
    public class GetReworkRateQueryHandler : IRequestHandler<GetReworkRateQuery, ErrorOr<ReworkRateDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetReworkRateQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<ReworkRateDto>> Handle(GetReworkRateQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetReworkRateAsync(cancellationToken);
            return result;
        }
    }
}
