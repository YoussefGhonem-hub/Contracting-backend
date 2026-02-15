using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Command.UpdateConstructionItem
{
    public class UpdateConstructionItemCommandHandler : IRequestHandler<UpdateConstructionItemCommand, ErrorOr<GetConstructionItemDto>>
    {
        private readonly IConstructionItemService _service;

        public UpdateConstructionItemCommandHandler(IConstructionItemService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetConstructionItemDto>> Handle(UpdateConstructionItemCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateAsync(request.Dto);
            return result is null
                ? Error.NotFound("ConstructionItem not found.")
                : result;
        }
    }
}
