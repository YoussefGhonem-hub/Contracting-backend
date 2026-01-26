using Contracting.Domain.Entities.helper;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Domain.Entities;

public class ApplicationUser : IdentityUser<Guid>
{
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public UserSignature? Signature { get; set; }
}
