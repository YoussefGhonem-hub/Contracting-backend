using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Query.GetClientVariationOrderById;

public class GetClientVariationOrderByIdQueryHandler : IRequestHandler<GetClientVariationOrderByIdQuery, ErrorOr<GetClientVariationOrderDetailDto>>
{
    private readonly IClientVariationOrderService _service;

    public GetClientVariationOrderByIdQueryHandler(IClientVariationOrderService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientVariationOrderDetailDto>> Handle(GetClientVariationOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetVariationOrderByIdAsync(request.VOId, cancellationToken);

        if (result is null)
            return Error.NotFound("VO.NotFound", "Variation order not found or not accessible.");

        return result;
    }
}
