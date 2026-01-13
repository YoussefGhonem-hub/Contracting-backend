using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestCountByStatus
{
    public class GetEngineerRequestCountByStatusQueryHandler : IRequestHandler<GetEngineerRequestCountByStatusQuery, ErrorOr<List<GetEngineerRequestCountByStatusDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetEngineerRequestCountByStatusQueryHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetEngineerRequestCountByStatusDto>>> Handle(GetEngineerRequestCountByStatusQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetEngineerRequestCountByStatusAsync(request.EngineerId);
            
            return result;
        }
    }
}
