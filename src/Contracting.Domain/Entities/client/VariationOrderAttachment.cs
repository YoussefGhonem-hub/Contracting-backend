using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class VariationOrderAttachment : BaseAuditableEntity
{
    public Guid VariationOrderId { get; set; }
    public VariationOrder VariationOrder { get; set; } = null!;
    public string? Key { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
