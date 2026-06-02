using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.TransferRequest.Query.GetTransferRequestById
{
    public class GetTransferRequestByIdQueryHandler : IRequestHandler<GetTransferRequestByIdQuery, ErrorOr<GetTransferRequestDto>>
    {
        private readonly ITransferRequestService _service;
        public GetTransferRequestByIdQueryHandler(ITransferRequestService service) => _service = service;
        public Task<ErrorOr<GetTransferRequestDto>> Handle(GetTransferRequestByIdQuery request, CancellationToken cancellationToken)
            => _service.GetByIdAsync(request.Id);
    }
}
