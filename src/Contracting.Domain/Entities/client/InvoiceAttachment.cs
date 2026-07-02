using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class InvoiceAttachment : BaseAuditableEntity
{
    public Guid ProjectInvoiceId { get; set; }
    public ProjectInvoice ProjectInvoice { get; set; } = null!;
    public string? Key { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
