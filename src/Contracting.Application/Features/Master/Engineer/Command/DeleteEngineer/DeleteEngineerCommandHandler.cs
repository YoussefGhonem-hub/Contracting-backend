using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.DeleteEngineer
{
    public class DeleteEngineerCommandHandler : IRequestHandler<DeleteEngineerCommand, ErrorOr<GenericResponse>>
    {
        private readonly IEngineerService _service;

        public DeleteEngineerCommandHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteEngineerCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteEngineerAsync(request.EngineerId);
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Engineer not found.");
        }
    }
}
