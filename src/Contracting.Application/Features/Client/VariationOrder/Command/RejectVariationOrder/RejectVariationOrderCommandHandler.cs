using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Command.RejectVariationOrder;

public class RejectVariationOrderCommandHandler : IRequestHandler<RejectVariationOrderCommand, ErrorOr<GetClientVariationOrderDetailDto>>
{
    private readonly IClientVariationOrderService _service;

    public RejectVariationOrderCommandHandler(IClientVariationOrderService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientVariationOrderDetailDto>> Handle(RejectVariationOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.RejectVariationOrderAsync(request.VOId, request.RejectionReason, cancellationToken: cancellationToken);

        if (result is null)
            return Error.Validation("VO.CannotReject", "Variation order not found, not accessible, or is not in Pending status.");

        return result;
    }
}
