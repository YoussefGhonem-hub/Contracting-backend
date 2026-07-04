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
        /// Actual received quantities per ConstructionItem-type special field item.
        /// Required when IsPartialReceipt = true.
        /// When IsPartialReceipt = false (full receipt), all items are automatically set to their full quantity.
        /// </summary>
        public List<EngineerRequestSpecialFieldItemReceiptDto> SpecialFieldItems { get; set; } = new();

        /// <summary>
        /// Actual received quantities for generic list-group fields that represent a quantity
        /// (e.g. "Qty Required" rows in a Procurement materials list configured via ListGroupKey
        /// instead of the ConstructionItem catalog). ItemId is the EngineerRequestSpecialFieldListItem's
        /// own row id. Required when IsPartialReceipt = true and the request has such rows.
        /// </summary>
        public List<EngineerRequestSpecialFieldItemReceiptDto> SpecialFieldListItems { get; set; } = new();
    }
}
