using Contracting.Shared.Common.Enums;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Command.UpdateProjectStatus;

public record UpdateProjectStatusCommand(Guid ProjectId, ProjectStatus NewStatus) : IRequest<ErrorOr<GetProjectDto>>;
