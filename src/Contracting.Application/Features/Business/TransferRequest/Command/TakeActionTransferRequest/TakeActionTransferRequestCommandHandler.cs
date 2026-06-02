using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.TakeActionTransferRequest
{
    public class TakeActionTransferRequestCommandHandler : IRequestHandler<TakeActionTransferRequestCommand, ErrorOr<GetTransferRequestDto>>
    {
        private readonly ITransferRequestService _service;
        public TakeActionTransferRequestCommandHandler(ITransferRequestService service) => _service = service;
        public Task<ErrorOr<GetTransferRequestDto>> Handle(TakeActionTransferRequestCommand request, CancellationToken cancellationToken)
            => _service.TakeActionAsync(request.Id, request.ActionDto);
    }
}
