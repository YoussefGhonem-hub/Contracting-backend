using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer
{
    public class UpdateEngineerCommandHandler : IRequestHandler<UpdateEngineerCommand, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;

        public UpdateEngineerCommandHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(UpdateEngineerCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateEngineerAsync(request.Engineer);
            return result is null
                ? Error.NotFound("Engineer not found.")
                : result;
        }
    }
}
