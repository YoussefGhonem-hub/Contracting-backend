using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.UpdateTransferRequest
{
    public class UpdateTransferRequestCommandHandler : IRequestHandler<UpdateTransferRequestCommand, ErrorOr<GetTransferRequestDto>>
    {
        private readonly ITransferRequestService _service;
        public UpdateTransferRequestCommandHandler(ITransferRequestService service) => _service = service;
        public Task<ErrorOr<GetTransferRequestDto>> Handle(UpdateTransferRequestCommand request, CancellationToken cancellationToken)
            => _service.UpdateAsync(request.Dto);
    }
}
