using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer
{
    public record UpdateEngineerCommand(UpdateEngineerDto Engineer) : IRequest<ErrorOr<GetEngineerDto>>;
}
