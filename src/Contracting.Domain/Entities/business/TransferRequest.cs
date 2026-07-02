using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class TransferRequest : BaseAuditableEntity
    {
        public string? RequestNumber { get; set; }
        public DateTime RequestDate { get; set; }
        public Guid? SourceProjectId { get; set; }
        public Project? SourceProject { get; set; }
        public string? SourceWarehouse { get; set; }
        public Guid? DestinationProjectId { get; set; }
        public Project? DestinationProject { get; set; }
        public string? DestinationWarehouse { get; set; }
        public Guid? RequestedById { get; set; }
        public Engineer? RequestedBy { get; set; }
        public string? Notes { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }

        // Set when status changes to Completed and department has NotifyOnTransferComplete = true.
        // Cleared when the creator acknowledges the completion.
        public bool NeedsAcknowledgment { get; set; }

        public ICollection<TransferRequestItem> Items { get; set; } = new List<TransferRequestItem>();
        public ICollection<TransferRequestAttachment> Attachments { get; set; } = new List<TransferRequestAttachment>();
        public ICollection<TransferRequestActivity> Activities { get; set; } = new List<TransferRequestActivity>();
    }
}
