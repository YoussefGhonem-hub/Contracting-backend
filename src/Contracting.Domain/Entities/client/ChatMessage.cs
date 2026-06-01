using Contracting.Domain.Common;
using Contracting.Domain.Common.Enums;

namespace Contracting.Domain.Entities.client;

public class ChatMessage : BaseAuditableEntity
{
    public Guid ChatGroupId { get; set; }
    public ChatGroup ChatGroup { get; set; } = null!;
    public Guid SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
    public string? Content { get; set; }
    public bool IsRead { get; set; }
    public ChatMessageType MessageType { get; set; } = ChatMessageType.Text;
    public ICollection<ChatMessageAttachment> Attachments { get; set; } = new List<ChatMessageAttachment>();
}
