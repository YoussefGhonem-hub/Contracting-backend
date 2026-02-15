using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Query.GetConstructionItemById
{
    public class GetConstructionItemByIdQueryHandler : IRequestHandler<GetConstructionItemByIdQuery, ErrorOr<GetConstructionItemDto>>
    {
        private readonly IConstructionItemService _service;

        public GetConstructionItemByIdQueryHandler(IConstructionItemService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetConstructionItemDto>> Handle(GetConstructionItemByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetByIdAsync(request.Id);
            return result is null
                ? Error.NotFound("ConstructionItem not found.")
                : result;
        }
    }
}
