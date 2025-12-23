using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.DeletePriority
{
    public class DeletePriorityCommandHandler : IRequestHandler<DeletePriorityCommand, ErrorOr<bool>>
    {
        private readonly IPriorityService _service;

        public DeletePriorityCommandHandler(IPriorityService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(DeletePriorityCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeletePriorityAsync(request.PriorityId);
            
            return result
                ? result
                : Error.NotFound("Priority not found.");
        }
    }
}