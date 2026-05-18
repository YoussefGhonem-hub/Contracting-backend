using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class PurchaseRequestReceipt : BaseAuditableEntity
    {
        public Guid EngineerRequestId { get; set; }
        public EngineerRequest? EngineerRequest { get; set; }

        public Guid? ReceivedById { get; set; }
        public Engineer? ReceivedBy { get; set; }

        public DateTime ReceiptDate { get; set; }
        public bool IsPartialReceipt { get; set; }  // true = partial, false = full
        public bool IsConfirmed { get; set; }
        public string? Notes { get; set; }
    }
}
