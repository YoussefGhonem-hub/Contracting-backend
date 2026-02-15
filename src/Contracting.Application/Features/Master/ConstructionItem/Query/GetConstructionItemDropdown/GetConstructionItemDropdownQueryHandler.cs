using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Query.GetConstructionItemDropdown
{
    public class GetConstructionItemDropdownQueryHandler : IRequestHandler<GetConstructionItemDropdownQuery, ErrorOr<List<GetConstructionItemDropdownDto>>>
    {
        private readonly IConstructionItemService _service;

        public GetConstructionItemDropdownQueryHandler(IConstructionItemService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<List<GetConstructionItemDropdownDto>>> Handle(GetConstructionItemDropdownQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetDropdownAsync();
            return result;
        }
    }
}
