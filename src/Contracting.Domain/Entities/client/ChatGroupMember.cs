using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class ChatGroupMember : BaseAuditableEntity
{
    public Guid ChatGroupId { get; set; }
    public ChatGroup ChatGroup { get; set; } = null!;
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public string MemberType { get; set; } = "TeamMember"; // "Client" | "TeamMember"
}
