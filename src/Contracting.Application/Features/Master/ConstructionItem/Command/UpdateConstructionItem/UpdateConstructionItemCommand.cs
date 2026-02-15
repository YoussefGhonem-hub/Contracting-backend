using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Command.UpdateConstructionItem
{
    public record UpdateConstructionItemCommand(UpdateConstructionItemDto Dto) : IRequest<ErrorOr<GetConstructionItemDto>>;
}
