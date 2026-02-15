using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Command.CreateConstructionItem
{
    public class CreateConstructionItemCommandHandler : IRequestHandler<CreateConstructionItemCommand, ErrorOr<GetConstructionItemDto>>
    {
        private readonly IConstructionItemService _service;

        public CreateConstructionItemCommandHandler(IConstructionItemService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetConstructionItemDto>> Handle(CreateConstructionItemCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateAsync(request.Dto);
            return result is null
                ? Error.Failure("ConstructionItem creation failed.")
                : result;
        }
    }
}
