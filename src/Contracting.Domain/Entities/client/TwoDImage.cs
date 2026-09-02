using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class TwoDImage : BaseAuditableEntity
{
    public Guid FolderId { get; set; }
    public TwoDFolder Folder { get; set; } = null!;

    public string? Key       { get; set; }
    public string? FileName  { get; set; }
    public string? Extension { get; set; }
    public long?   FileSize  { get; set; }
    public string? Url       { get; set; }

    public Guid UploadedBy { get; set; }
    public ApplicationUser UploadedByUser { get; set; } = null!;
}
