using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Command.CreateConstructionItem
{
    public record CreateConstructionItemCommand(CreateConstructionItemDto Dto) : IRequest<ErrorOr<GetConstructionItemDto>>;
}
