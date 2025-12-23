using Contracting.Infrustructure.Inteface.business;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public class TakeActionOnRequestCommandHandler : IRequestHandler<TakeActionOnRequestCommand, ErrorOr<bool>>
    {
        private readonly IEngineerRequestService _service;

        public TakeActionOnRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(TakeActionOnRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.TakeActionOnRequestAsync(
                request.RequestId, 
                request.EngineerId, 
                request.IsApproved, 
                request.ActionNote);
            
            return result
                ? result
                : Error.Validation("Could not take action. Request may have been actioned already or engineer is not authorized.");
        }
    }
}