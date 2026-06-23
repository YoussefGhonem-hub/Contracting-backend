using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class ChatGroupMember : BaseAuditableEntity
{
    public Guid ChatGroupId { get; set; }
    public ChatGroup ChatGroup { get; set; } = null!;
    public Guid ApplicationUserId { get; set; }
    public ApplicationUser ApplicationUser { get; set; } = null!;
    public string MemberType { get; set; } = "TeamMember"; // "Client" | "TeamMember"

    /// <summary>
    /// Timestamp of the last time this member read the group's messages. Per-user read tracking:
    /// any message created after this (and not sent by the member) counts as unread.
    /// Null means the member has never opened the chat — everything is unread.
    /// </summary>
    public DateTimeOffset? LastReadAt { get; set; }
}
