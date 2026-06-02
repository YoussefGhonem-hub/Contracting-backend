using Contracting.Shared.Dtos.BusinessDtos.VariationOrderDtos;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;

namespace Contracting.Infrustructure.Inteface.business;

public interface ITechnicalVariationOrderService
{
    Task<GetClientVariationOrderDetailDto?> CreateVariationOrderAsync(CreateVariationOrderDto dto, CancellationToken cancellationToken = default);
}
