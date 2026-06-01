using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class ProjectSchedule : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Key { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
    public Guid UploadedBy { get; set; }
    public ApplicationUser UploadedByUser { get; set; } = null!;
}
