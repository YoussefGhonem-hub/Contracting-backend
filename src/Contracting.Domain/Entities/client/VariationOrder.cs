using Contracting.Domain.Common;
using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class VariationOrder : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public VOStatus Status { get; set; } = VOStatus.Pending;
    public Guid CreatedByEngineerId { get; set; }
    public Engineer CreatedByEngineer { get; set; } = null!;
    public DateTimeOffset? ClientActionDate { get; set; }
    public string? ClientRejectionReason { get; set; }
    public ICollection<VariationOrderAttachment> Attachments { get; set; } = new List<VariationOrderAttachment>();
}
