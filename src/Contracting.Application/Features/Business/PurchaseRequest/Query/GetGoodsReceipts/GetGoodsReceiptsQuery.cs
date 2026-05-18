using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.PurchaseRequest.Query.GetGoodsReceipts
{
    public record GetGoodsReceiptsQuery(Guid RequestId)
        : IRequest<ErrorOr<List<GetGoodsReceiptDto>>>;
}
