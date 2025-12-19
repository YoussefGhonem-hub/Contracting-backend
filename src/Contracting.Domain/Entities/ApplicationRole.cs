using Microsoft.AspNetCore.Identity;

namespace Contracting.Domain.Entities;
public class ApplicationRole : IdentityRole<Guid>
{
    public string? DisplayName { get; set; }
}
