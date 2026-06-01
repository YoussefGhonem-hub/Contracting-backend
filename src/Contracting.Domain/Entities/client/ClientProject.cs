using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class ClientProject : BaseAuditableEntity
{
    public Guid ClientId { get; set; }
    public Client Client { get; set; } = null!;
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
}
