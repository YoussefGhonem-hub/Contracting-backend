using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.DeletePriority
{
    public class DeletePriorityCommandHandler : IRequestHandler<DeletePriorityCommand, ErrorOr<GenericResponse>>
    {
        private readonly IPriorityService _service;

        public DeletePriorityCommandHandler(IPriorityService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeletePriorityCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeletePriorityAsync(request.PriorityId);
            
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Priority not found.");
        }
    }
}