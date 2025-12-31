using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Query.GetAllPriority
{
    public record GetAllPriorityQuery(BaseFilterDto Filter)
        : IRequest<ErrorOr<PaginatedList<GetDropDownPriorityDto>>>;
}
