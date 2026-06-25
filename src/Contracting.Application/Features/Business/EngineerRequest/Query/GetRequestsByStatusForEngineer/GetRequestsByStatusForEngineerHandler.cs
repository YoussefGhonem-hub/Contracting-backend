using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestsByStatusForEngineer
{
    public class GetRequestsByStatusForEngineerHandler : IRequestHandler<GetRequestsByStatusForEngineerQuery, ErrorOr<GetRequestsByStatusResponseDto>>
    {
        private readonly IEngineerRequestService _service;

        public GetRequestsByStatusForEngineerHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetRequestsByStatusResponseDto>> Handle(GetRequestsByStatusForEngineerQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetRequestsByStatusForEngineerAsync(request.Filter, cancellationToken);
        }
    }
}
