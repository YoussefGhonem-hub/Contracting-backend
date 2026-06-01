using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class ClientMonthlyReport : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public int Month { get; set; }
    public int Year { get; set; }
    public string? Title { get; set; }
    public string? WorkProgress { get; set; }
    public Guid UploadedBy { get; set; }
    public ApplicationUser UploadedByUser { get; set; } = null!;
    public ICollection<ClientMonthlyReportAttachment> Attachments { get; set; } = new List<ClientMonthlyReportAttachment>();
}
