namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class CreateTransferRequestItemDto
    {
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public string? Notes { get; set; }
    }
}
