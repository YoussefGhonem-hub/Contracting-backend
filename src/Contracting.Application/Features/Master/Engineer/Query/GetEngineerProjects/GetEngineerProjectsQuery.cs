using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerProjects
{
    public record GetEngineerProjectsQuery(Guid EngineerId) : IRequest<ErrorOr<List<GetEngineerProjectDto>>>;
}
