using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetOfficeEngineerAnalysis
{
    public class GetOfficeEngineerAnalysisQueryHandler : IRequestHandler<GetOfficeEngineerAnalysisQuery, ErrorOr<OfficeEngineerAnalysisDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetOfficeEngineerAnalysisQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<OfficeEngineerAnalysisDto>> Handle(GetOfficeEngineerAnalysisQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetOfficeEngineerAnalysisAsync(cancellationToken);
            return result;
        }
    }
}
