using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetSiteEngineerAnalysis
{
    public class GetSiteEngineerAnalysisQueryHandler : IRequestHandler<GetSiteEngineerAnalysisQuery, ErrorOr<SiteEngineerAnalysisDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetSiteEngineerAnalysisQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<SiteEngineerAnalysisDto>> Handle(GetSiteEngineerAnalysisQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetSiteEngineerAnalysisAsync(cancellationToken);
            return result;
        }
    }
}
