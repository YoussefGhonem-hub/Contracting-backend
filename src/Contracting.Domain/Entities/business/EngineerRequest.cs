using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequest : BaseAuditableEntity
    {
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? PriorityId { get; set; }
        public Priority? Priority { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public string? RequestTitle { get; set; }
        public string? Descreption { get; set; }
        public int timeDuration { get; set; }
        public Guid StatusId { get; set; }
        public Status Status { get; set; }
        public Guid? assignToId { get; set; }
        public Engineer? assignTo { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public bool IsDeliveryDateConfirmed { get; set; }
        // True when office engineer marks as complete but site engineer hasn't confirmed receipt yet
        public bool NeedsReceiptConfirmation { get; set; }
        public ICollection<EngineerRequestNotes> EngineerRequestNotes { get; set; }
        public ICollection<EngineerRequestActivite> EngineerRequestActivites { get; set; }
        public ICollection<EngineerRequestAttachment> EngineerRequestAttachments { get; set; }
        public ICollection<EngineerRequestSpecialFieldValue> SpecialFieldValues { get; set; } = new List<EngineerRequestSpecialFieldValue>();
        public ICollection<EngineerRequestSpecialFieldItem> SpecialFieldItems { get; set; } = new List<EngineerRequestSpecialFieldItem>();

        // Goods receipt records (Purchase Request workflow)
        public ICollection<PurchaseRequestReceipt> PurchaseReceipts { get; set; } = new List<PurchaseRequestReceipt>();
    }
}
