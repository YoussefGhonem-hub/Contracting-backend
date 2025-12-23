using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Project.Query.GetAllProjects
{
    public record GetAllProjectsQuery(Guid? BranchId, BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetProjectDto>>>;
}