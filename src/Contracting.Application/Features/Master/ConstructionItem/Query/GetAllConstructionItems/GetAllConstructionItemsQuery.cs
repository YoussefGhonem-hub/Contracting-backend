using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Query.GetAllConstructionItems
{
    public record GetAllConstructionItemsQuery(BaseFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetConstructionItemDto>>>;
}
