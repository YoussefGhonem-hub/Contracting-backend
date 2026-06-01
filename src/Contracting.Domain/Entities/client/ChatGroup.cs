using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.client;

public class ChatGroup : BaseAuditableEntity
{
    public Guid ProjectId { get; set; }
    public Project Project { get; set; } = null!;
    public string? Name { get; set; }
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    public ICollection<ChatGroupMember> Members { get; set; } = new List<ChatGroupMember>();
}
