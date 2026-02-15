using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Query.GetConstructionItemDropdown
{
    public record GetConstructionItemDropdownQuery() : IRequest<ErrorOr<List<GetConstructionItemDropdownDto>>>;
}
