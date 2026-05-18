using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.PurchaseRequest.Command.CreateGoodsReceipt
{
    public class CreateGoodsReceiptCommandHandler
        : IRequestHandler<CreateGoodsReceiptCommand, ErrorOr<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;

        public CreateGoodsReceiptCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public Task<ErrorOr<GetAllEngineerRequestDto>> Handle(CreateGoodsReceiptCommand request, CancellationToken cancellationToken)
            => _service.CreateGoodsReceiptAsync(request.RequestId, request.Receipt);
    }
}
