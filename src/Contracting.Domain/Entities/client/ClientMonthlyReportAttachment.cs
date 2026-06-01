using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class ClientMonthlyReportAttachment : BaseAuditableEntity
{
    public Guid ClientMonthlyReportId { get; set; }
    public ClientMonthlyReport ClientMonthlyReport { get; set; } = null!;
    public string? Key { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
