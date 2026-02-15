using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Query.GetConstructionItemById
{
    public record GetConstructionItemByIdQuery(Guid Id) : IRequest<ErrorOr<GetConstructionItemDto>>;
}
