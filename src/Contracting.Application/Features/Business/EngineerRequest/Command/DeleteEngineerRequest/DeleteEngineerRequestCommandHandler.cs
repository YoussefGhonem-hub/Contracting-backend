using Contracting.Infrustructure.Inteface.business;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest
{
    public class DeleteEngineerRequestCommandHandler : IRequestHandler<DeleteEngineerRequestCommand, ErrorOr<bool>>
    {
        private readonly IEngineerRequestService _service;

        public DeleteEngineerRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(DeleteEngineerRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteEngineerRequestAsync(request.RequestId);
            
            return result
                ? result
                : Error.NotFound("Engineer request not found or has already been actioned.");
        }
    }
}