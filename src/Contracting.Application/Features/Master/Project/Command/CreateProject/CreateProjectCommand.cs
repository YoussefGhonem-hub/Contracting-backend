using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.CreateProject
{
    public record CreateProjectCommand(CreateProjectDto Project) : IRequest<ErrorOr<GetProjectDto>>;
}