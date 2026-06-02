using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Client.VariationOrder.Query.GetClientVariationOrders;

public class GetClientVariationOrdersQueryHandler : IRequestHandler<GetClientVariationOrdersQuery, ErrorOr<GetClientVariationOrdersDto>>
{
    private readonly IClientVariationOrderService _service;

    public GetClientVariationOrdersQueryHandler(IClientVariationOrderService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientVariationOrdersDto>> Handle(GetClientVariationOrdersQuery request, CancellationToken cancellationToken)
    {
        var result = await _service.GetVariationOrdersAsync(request.ProjectId, request.Status, cancellationToken: cancellationToken);

        if (result is null)
            return Error.NotFound("VO.ProjectNotFound", "Project not found or not assigned to this client.");

        return result;
    }
}
