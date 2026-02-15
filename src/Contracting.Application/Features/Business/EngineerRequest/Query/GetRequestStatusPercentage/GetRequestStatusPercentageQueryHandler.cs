using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentage
{
    public class GetRequestStatusPercentageQueryHandler : IRequestHandler<GetRequestStatusPercentageQuery, ErrorOr<EngineerStatusPercentageReportDto>>
    {
        private readonly IEngineerRequestAnalysisService _service;

        public GetRequestStatusPercentageQueryHandler(IEngineerRequestAnalysisService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<EngineerStatusPercentageReportDto>> Handle(GetRequestStatusPercentageQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetRequestStatusPercentageAsync(request.StartDate, request.EndDate, cancellationToken);
            return result;
        }
    }
}
