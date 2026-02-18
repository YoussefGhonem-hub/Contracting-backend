using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetWeeklyCompletion
{
    public class GetWeeklyCompletionQueryHandler : IRequestHandler<GetWeeklyCompletionQuery, ErrorOr<WeeklyCompletionReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetWeeklyCompletionQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<WeeklyCompletionReportDto>> Handle(GetWeeklyCompletionQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetWeeklyCompletionAsync(request.Month, request.Year, cancellationToken);
            return result;
        }
    }
}
