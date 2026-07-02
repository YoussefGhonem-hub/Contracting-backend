using Contracting.Domain.Common;
using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class ProjectInvoice : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int? InvoiceNumber { get; set; }
    public string? Title { get; set; }
    public decimal TotalValue { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? Notes { get; set; }
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public Guid UpdatedBy { get; set; }
    public ApplicationUser UpdatedByUser { get; set; } = null!;
    public ICollection<InvoicePayment> Payments { get; set; } = new List<InvoicePayment>();
    public ICollection<InvoiceAttachment> Attachments { get; set; } = new List<InvoiceAttachment>();
}
