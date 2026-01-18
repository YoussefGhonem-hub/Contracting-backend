using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetSlaBucketsReport
{
    public class GetSlaBucketsReportQueryHandler : IRequestHandler<GetSlaBucketsReportQuery, ErrorOr<SlaBucketsReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetSlaBucketsReportQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<SlaBucketsReportDto>> Handle(GetSlaBucketsReportQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetSlaBucketsByPriorityAndDepartmentAsync(cancellationToken);
            return result;
        }
    }
}
