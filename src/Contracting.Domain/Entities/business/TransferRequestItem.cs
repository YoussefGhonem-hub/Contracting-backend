using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class TransferRequestItem : BaseAuditableEntity
    {
        public Guid TransferRequestId { get; set; }
        public TransferRequest? TransferRequest { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Unit { get; set; }
        public decimal Quantity { get; set; }
        public decimal? ReceivedQuantity { get; set; }
        public string? Notes { get; set; }
    }
}
