using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common.Enums;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetAllProjects
{
    public record GetAllProjectsQuery(Guid? BranchId, BaseFilterDto Filter, ProjectStatus? Status = null) : IRequest<ErrorOr<PaginatedList<GetProjectDto>>>;
}