using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.PurchaseRequest.Query.GetGoodsReceipts
{
    public class GetGoodsReceiptsQueryHandler
        : IRequestHandler<GetGoodsReceiptsQuery, ErrorOr<List<GetGoodsReceiptDto>>>
    {
        private readonly IEngineerRequestService _service;

        public GetGoodsReceiptsQueryHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public Task<ErrorOr<List<GetGoodsReceiptDto>>> Handle(GetGoodsReceiptsQuery request, CancellationToken cancellationToken)
            => _service.GetGoodsReceiptsAsync(request.RequestId);
    }
}
