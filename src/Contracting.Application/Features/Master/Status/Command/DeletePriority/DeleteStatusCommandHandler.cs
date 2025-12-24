using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.DeleteStatus
{
    public class DeleteStatusCommandHandler : IRequestHandler<DeleteStatusCommand, ErrorOr<bool>>
    {
        private readonly IStatueService _service;

        public DeleteStatusCommandHandler(IStatueService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(DeleteStatusCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteStatusAsync(request.StatusId);
            
            return result
                ? result
                : Error.NotFound("Status not found.");
        }
    }
}