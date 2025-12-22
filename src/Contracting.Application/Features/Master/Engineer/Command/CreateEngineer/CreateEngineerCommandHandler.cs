using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.CreateEngineer
{
    public class CreateEngineerCommandHandler : IRequestHandler<CreateEngineerCommand, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;

        public CreateEngineerCommandHandler(IEngineerService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(CreateEngineerCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateEngineerAsync(request.Engineer);
            return result is null
                ? Error.Failure("Could not create engineer.")
                : result;
        }
    }
}
