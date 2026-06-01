using Contracting.Domain.Common;
using Contracting.Domain.Common.Enums;

namespace Contracting.Domain.Entities.client;

public class ChatMessageAttachment : BaseAuditableEntity
{
    public Guid ChatMessageId { get; set; }
    public ChatMessage ChatMessage { get; set; } = null!;
    public string? Key { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
    public ChatMessageType AttachmentType { get; set; } = ChatMessageType.Document;
}
