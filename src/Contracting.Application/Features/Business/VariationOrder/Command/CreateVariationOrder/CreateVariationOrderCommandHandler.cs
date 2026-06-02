using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.VariationOrder.Command.CreateVariationOrder;

public class CreateVariationOrderCommandHandler : IRequestHandler<CreateVariationOrderCommand, ErrorOr<GetClientVariationOrderDetailDto>>
{
    private readonly ITechnicalVariationOrderService _service;

    public CreateVariationOrderCommandHandler(ITechnicalVariationOrderService service)
    {
        _service = service;
    }

    public async Task<ErrorOr<GetClientVariationOrderDetailDto>> Handle(CreateVariationOrderCommand request, CancellationToken cancellationToken)
    {
        var result = await _service.CreateVariationOrderAsync(request.Dto, cancellationToken);

        if (result is null)
            return Error.Validation("VariationOrder.CreateFailed", "Unable to create variation order. Check project and technical-office assignment.");

        return result;
    }
}
