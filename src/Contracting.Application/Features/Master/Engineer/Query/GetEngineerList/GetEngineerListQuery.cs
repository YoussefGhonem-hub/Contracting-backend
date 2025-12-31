using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerList
{
    public record GetEngineerListQuery(Guid DepartmentId, BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetEngineerDto>>>;
}
