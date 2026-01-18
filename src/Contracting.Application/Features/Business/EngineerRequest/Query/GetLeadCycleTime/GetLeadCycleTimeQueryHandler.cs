using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetLeadCycleTime
{
    public class GetLeadCycleTimeQueryHandler : IRequestHandler<GetLeadCycleTimeQuery, ErrorOr<LeadCycleTimeDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetLeadCycleTimeQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<LeadCycleTimeDto>> Handle(GetLeadCycleTimeQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetLeadAndCycleTimeAsync(cancellationToken);
            return result;
        }
    }
}
