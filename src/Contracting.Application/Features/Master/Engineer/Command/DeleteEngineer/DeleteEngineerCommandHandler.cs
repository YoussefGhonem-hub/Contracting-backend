using Contracting.Infrustructure.Inteface;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.DeleteEngineer
{
    public class DeleteEngineerCommandHandler : IRequestHandler<DeleteEngineerCommand, ErrorOr<bool>>
    {
        private readonly IEngineerService _service;

        public DeleteEngineerCommandHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(DeleteEngineerCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteEngineerAsync(request.EngineerId);
            return result
                ? true
                : Error.NotFound("Engineer not found.");
        }
    }
}
