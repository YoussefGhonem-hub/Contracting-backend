namespace Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;

public class GetClientVariationOrdersDto
{
    public decimal TotalApproved { get; set; }
    public decimal TotalPending { get; set; }
    public List<GetClientVariationOrderListItemDto> VariationOrders { get; set; } = new();
}
