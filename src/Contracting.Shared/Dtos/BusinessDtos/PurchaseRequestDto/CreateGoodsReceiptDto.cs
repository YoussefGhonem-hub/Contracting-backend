namespace Contracting.Shared.BusinessDtos.PurchaseRequestDto
{
    public class CreateGoodsReceiptDto
    {
        public DateTime? ReceiptDate { get; set; }
        /// <summary>true = partial receipt, false = full receipt</summary>
        public bool IsPartialReceipt { get; set; }
        /// <summary>Confirmed = close the request regardless of full/partial</summary>
        public bool IsConfirmed { get; set; }
        public string? Notes { get; set; }
    }
}
