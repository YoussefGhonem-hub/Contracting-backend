using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest
{
    public class DeleteEngineerRequestCommandHandler : IRequestHandler<DeleteEngineerRequestCommand, ErrorOr<GenericResponse>>
    {
        private readonly IEngineerRequestService _service;

        public DeleteEngineerRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteEngineerRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteEngineerRequestAsync(request.RequestId);
            
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Engineer request not found or has already been actioned.");
        }
    }
}