using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Command.CreateTransferRequest
{
    public class CreateTransferRequestCommandHandler : IRequestHandler<CreateTransferRequestCommand, ErrorOr<GetTransferRequestDto>>
    {
        private readonly ITransferRequestService _service;
        public CreateTransferRequestCommandHandler(ITransferRequestService service) => _service = service;
        public Task<ErrorOr<GetTransferRequestDto>> Handle(CreateTransferRequestCommand request, CancellationToken cancellationToken)
            => _service.CreateAsync(request.Dto);
    }
}
