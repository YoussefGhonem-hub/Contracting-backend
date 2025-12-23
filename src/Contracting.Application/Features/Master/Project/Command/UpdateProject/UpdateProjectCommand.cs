using Contracting.Shared.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.UpdateProject
{
    public record UpdateProjectCommand(UpdateProjectDto Project) : IRequest<ErrorOr<GetProjectDto>>;
}