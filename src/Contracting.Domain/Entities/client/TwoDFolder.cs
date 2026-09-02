using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class TwoDFolder : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public string Title { get; set; } = default!;

    public string? CoverKey { get; set; }
    public string? CoverUrl { get; set; }

    public Guid UploadedBy { get; set; }
    public ApplicationUser UploadedByUser { get; set; } = null!;

    public ICollection<TwoDImage> Images { get; set; } = new List<TwoDImage>();
}
