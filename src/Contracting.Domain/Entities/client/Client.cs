using Contracting.Domain.Common;
using Contracting.Domain.Entities;

namespace Contracting.Domain.Entities.client;

public class Client : BaseAuditableEntity
{
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public string? CompanyName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Address { get; set; }
    public ICollection<ClientProject> ClientProjects { get; set; } = new List<ClientProject>();
}
