using ErrorOr;
using MediatR;
using System;

namespace Contracting.Application.Features.Master.Engineer.Command.DeleteEngineer
{
    public record DeleteEngineerCommand(Guid EngineerId) : IRequest<ErrorOr<bool>>;
}
