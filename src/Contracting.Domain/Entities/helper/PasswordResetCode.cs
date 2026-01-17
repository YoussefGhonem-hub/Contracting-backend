using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.helper;

public class PasswordResetCode : BaseAuditableEntity
{
    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTimeOffset ExpiresAt { get; set; }
    public bool IsVerified { get; set; } = false;
    public bool IsUsed { get; set; } = false;
    public DateTimeOffset? VerifiedAt { get; set; }
    public DateTimeOffset? UsedAt { get; set; }
    
    // Navigation
    public ApplicationUser User { get; set; } = null!;
}
