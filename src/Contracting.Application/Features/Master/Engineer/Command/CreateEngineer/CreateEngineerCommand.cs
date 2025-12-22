using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.CreateEngineer
{
    public record CreateEngineerCommand(CreateEngineerDto Engineer) : IRequest<ErrorOr<GetEngineerDto>>;
}
