using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.PurchaseRequest.Command.CreateGoodsReceipt
{
    public record CreateGoodsReceiptCommand(Guid RequestId, CreateGoodsReceiptDto Receipt)
        : IRequest<ErrorOr<GetAllEngineerRequestDto>>;
}
