using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Query.GetAllConstructionItems
{
    public class GetAllConstructionItemsQueryHandler : IRequestHandler<GetAllConstructionItemsQuery, ErrorOr<PaginatedList<GetConstructionItemDto>>>
    {
        private readonly IConstructionItemService _service;

        public GetAllConstructionItemsQueryHandler(IConstructionItemService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetConstructionItemDto>>> Handle(GetAllConstructionItemsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
