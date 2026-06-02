using Contracting.Domain.Common;
using Contracting.Domain.Entities.business.enums;
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
        public TransferRequestStatus Status { get; set; } = TransferRequestStatus.Draft;

        public ICollection<TransferRequestItem> Items { get; set; } = new List<TransferRequestItem>();
        public ICollection<TransferRequestAttachment> Attachments { get; set; } = new List<TransferRequestAttachment>();
        public ICollection<TransferRequestActivity> Activities { get; set; } = new List<TransferRequestActivity>();
    }
}
