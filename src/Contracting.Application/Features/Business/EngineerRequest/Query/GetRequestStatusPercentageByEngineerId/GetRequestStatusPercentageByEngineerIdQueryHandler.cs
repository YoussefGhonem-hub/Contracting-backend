using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentageByEngineerId
{
    public class GetRequestStatusPercentageByEngineerIdQueryHandler : IRequestHandler<GetRequestStatusPercentageByEngineerIdQuery, ErrorOr<EngineerStatusPercentageReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetRequestStatusPercentageByEngineerIdQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<EngineerStatusPercentageReportDto>> Handle(GetRequestStatusPercentageByEngineerIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetRequestStatusPercentageByEngineerIdAsync(request.EngineerId, request.Month, request.Year, cancellationToken);
            return result;
        }
    }
}
