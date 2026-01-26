using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.helper;

public class UserSignature : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string SignatureUrl { get; set; } = default!;
    public string FileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long FileSize { get; set; }
}
