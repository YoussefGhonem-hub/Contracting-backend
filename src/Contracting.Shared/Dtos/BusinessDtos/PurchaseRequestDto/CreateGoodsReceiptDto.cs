using Contracting.Shared.BusinessDtos.EngineerRequestDto;

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
        /// <summary>
        /// Actual received quantities per item.
        /// Required when IsPartialReceipt = true.
        /// When IsPartialReceipt = false (full receipt), all items are automatically set to their full quantity.
        /// </summary>
        public List<EngineerRequestSpecialFieldItemReceiptDto> SpecialFieldItems { get; set; } = new();
    }
}
