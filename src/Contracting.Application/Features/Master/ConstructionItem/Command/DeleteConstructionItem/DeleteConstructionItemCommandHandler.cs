using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.ConstructionItem.Command.DeleteConstructionItem
{
    public class DeleteConstructionItemCommandHandler : IRequestHandler<DeleteConstructionItemCommand, ErrorOr<GenericResponse>>
    {
        private readonly IConstructionItemService _service;

        public DeleteConstructionItemCommandHandler(IConstructionItemService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteConstructionItemCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.DeleteAsync(request.Id);
            return result;
        }
    }
}
