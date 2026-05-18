using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.DeleteTransferRequest
{
    public class DeleteTransferRequestCommandHandler : IRequestHandler<DeleteTransferRequestCommand, ErrorOr<GenericResponse>>
    {
        private readonly ITransferRequestService _service;
        public DeleteTransferRequestCommandHandler(ITransferRequestService service) => _service = service;
        public Task<ErrorOr<GenericResponse>> Handle(DeleteTransferRequestCommand request, CancellationToken cancellationToken)
            => _service.DeleteAsync(request.Id);
    }
}
