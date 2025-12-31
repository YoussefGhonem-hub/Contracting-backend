using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetProjectById
{
    public record GetProjectByIdQuery(Guid ProjectId) : IRequest<ErrorOr<GetProjectDto>>;
}