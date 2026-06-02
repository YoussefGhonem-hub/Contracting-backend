using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetDailyReportCompletion
{
    public class GetDailyReportCompletionQueryHandler : IRequestHandler<GetDailyReportCompletionQuery, ErrorOr<DailyReportCompletionDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetDailyReportCompletionQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<DailyReportCompletionDto>> Handle(GetDailyReportCompletionQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetDailyReportCompletionRateAsync(request.Month, request.Year, cancellationToken);
            return result;
        }
    }
}
