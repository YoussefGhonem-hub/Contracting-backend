using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineerProjectFeatures
{
    public record UpdateEngineerProjectFeaturesCommand(
        Guid EngineerId,
        Guid ProjectId,
        List<string> Features) : IRequest<ErrorOr<GetEngineerProjectDto>>;
}
