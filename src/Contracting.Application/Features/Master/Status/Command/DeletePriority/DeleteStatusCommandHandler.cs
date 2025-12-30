using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.DeleteStatus
{
    public class DeleteStatusCommandHandler : IRequestHandler<DeleteStatusCommand, ErrorOr<GenericResponse>>
    {
        private readonly IStatueService _service;

        public DeleteStatusCommandHandler(IStatueService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteStatusCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteStatusAsync(request.StatusId);
            
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Status not found.");
        }
    }
}