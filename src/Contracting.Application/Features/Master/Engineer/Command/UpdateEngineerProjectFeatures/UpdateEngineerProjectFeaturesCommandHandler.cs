using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineerProjectFeatures
{
    public class UpdateEngineerProjectFeaturesCommandHandler
        : IRequestHandler<UpdateEngineerProjectFeaturesCommand, ErrorOr<GetEngineerProjectDto>>
    {
        private readonly IEngineerService _service;

        public UpdateEngineerProjectFeaturesCommandHandler(IEngineerService service)
        {
            _service = service;
        }

        public Task<ErrorOr<GetEngineerProjectDto>> Handle(
            UpdateEngineerProjectFeaturesCommand request,
            CancellationToken cancellationToken)
            => _service.UpdateEngineerProjectFeaturesAsync(request.EngineerId, request.ProjectId, request.Features);
    }
}
