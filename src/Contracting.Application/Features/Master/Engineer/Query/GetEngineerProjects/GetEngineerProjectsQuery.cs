using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerProjects
{
    public record GetEngineerProjectsQuery(Guid EngineerId) : IRequest<ErrorOr<List<GetProjectDropDownDto>>>;
}
