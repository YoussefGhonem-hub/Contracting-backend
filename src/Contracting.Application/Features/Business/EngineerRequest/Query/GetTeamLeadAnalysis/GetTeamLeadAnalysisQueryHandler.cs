using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetTeamLeadAnalysis
{
    public class GetTeamLeadAnalysisQueryHandler : IRequestHandler<GetTeamLeadAnalysisQuery, ErrorOr<TeamLeadAnalysisDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetTeamLeadAnalysisQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<TeamLeadAnalysisDto>> Handle(GetTeamLeadAnalysisQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetTeamLeadAnalysisAsync(cancellationToken);
            return result;
        }
    }
}
