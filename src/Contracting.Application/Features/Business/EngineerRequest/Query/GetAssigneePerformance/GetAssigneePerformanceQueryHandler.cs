using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformance
{
    public class GetAssigneePerformanceQueryHandler : IRequestHandler<GetAssigneePerformanceQuery, ErrorOr<AssigneePerformanceReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetAssigneePerformanceQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<AssigneePerformanceReportDto>> Handle(GetAssigneePerformanceQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAssigneePerformanceAsync(cancellationToken);
            return result;
        }
    }
}
