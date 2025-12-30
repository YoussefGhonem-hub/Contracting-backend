using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.DeleteProject
{
    public record DeleteProjectCommand(Guid ProjectId) : IRequest<ErrorOr<GenericResponse>>;
}