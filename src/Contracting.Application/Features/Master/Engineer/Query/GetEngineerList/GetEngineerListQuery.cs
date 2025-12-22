using Contracting.Shared.MasterDtos.EngineerDto;
using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;
using Contracting.Shared.Dtos;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerList
{
    public record GetEngineerListQuery(string DepartmentId, BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetEngineerDto>>>;
}
