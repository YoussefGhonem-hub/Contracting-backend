using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Command.ApproveVariationOrder;

public class ApproveVariationOrderCommandHandler : IRequestHandler<ApproveVariationOrderCommand, ErrorOr<GetClientVariationOrderDetailDto>>
{
    private readonly IClientVariationOrderService _service;

    public ApproveVariationOrderCommandHandler(IClientVariationOrderService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientVariationOrderDetailDto>> Handle(ApproveVariationOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.ApproveVariationOrderAsync(request.VOId, cancellationToken);

        if (result is null)
            return Error.Validation("VO.CannotApprove", "Variation order not found, not accessible, or is not in Pending status.");

        return result;
    }
}
