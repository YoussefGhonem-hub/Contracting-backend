using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformanceByEngineerId
{
    public class GetAssigneePerformanceByEngineerIdQueryHandler : IRequestHandler<GetAssigneePerformanceByEngineerIdQuery, ErrorOr<AssigneePerformanceReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetAssigneePerformanceByEngineerIdQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<AssigneePerformanceReportDto>> Handle(GetAssigneePerformanceByEngineerIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAssigneePerformanceByEngineerIdAsync(request.EngineerId, cancellationToken);
            return result;
        }
    }
}
