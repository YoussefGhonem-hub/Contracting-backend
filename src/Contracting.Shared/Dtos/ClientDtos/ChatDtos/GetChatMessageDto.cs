namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

public class GetChatMessageDto
{
    public Guid Id { get; set; }
    public Guid SenderId { get; set; }
    public string? SenderName { get; set; }
    public string? SenderAvatarUrl { get; set; }
    public string? Content { get; set; }
    public string MessageType { get; set; } = "Text";
    public bool IsRead { get; set; }
    public DateTimeOffset SentAt { get; set; }
    public List<GetChatAttachmentDto> Attachments { get; set; } = new();
}

public class GetChatAttachmentDto
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
    public string? AttachmentType { get; set; }
}
