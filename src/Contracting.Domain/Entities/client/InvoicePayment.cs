using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class InvoicePayment : BaseAuditableEntity
{
    public Guid ProjectInvoiceId { get; set; }
    public ProjectInvoice ProjectInvoice { get; set; } = null!;
    public decimal Amount { get; set; }
    public DateTimeOffset PaymentDate { get; set; }
    public string? Reference { get; set; }
    public string? Notes { get; set; }
}
