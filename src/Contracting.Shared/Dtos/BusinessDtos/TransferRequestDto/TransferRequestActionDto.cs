namespace Contracting.Shared.BusinessDtos.TransferRequestDto
{
    public class TransferRequestActionDto
    {
        /// <summary>ConfirmReceipt, ConfirmPartialReceipt, Cancel</summary>
        public string ActionType { get; set; } = string.Empty;
        public string? Comments { get; set; }
        /// <summary>
        /// Per-item received quantities. Required for ConfirmPartialReceipt.
        /// Optional for ConfirmReceipt (defaults to full Quantity if omitted).
        /// </summary>
        public List<TransferRequestItemReceiptDto> Items { get; set; } = new();
    }

    public class TransferRequestItemReceiptDto
    {
        public Guid ItemId { get; set; }
        public decimal ReceivedQuantity { get; set; }
    }
}
