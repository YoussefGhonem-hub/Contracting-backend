namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class GetTransferRequestItemDto
    {
        public Guid Id { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
