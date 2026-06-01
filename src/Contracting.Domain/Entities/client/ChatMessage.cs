using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.client;

public class ChatMessage : BaseAuditableEntity
{
    public Guid ChatGroupId { get; set; }
    public ChatGroup ChatGroup { get; set; } = null!;
    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    public string? Content { get; set; }
    public bool IsRead { get; set; }
    public ICollection<ChatMessageAttachment> Attachments { get; set; } = new List<ChatMessageAttachment>();
}
