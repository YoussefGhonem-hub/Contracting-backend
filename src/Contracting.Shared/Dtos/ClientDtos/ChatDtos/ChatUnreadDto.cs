namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

/// <summary>
/// Unread summary for the current user across all chat groups they belong to.
/// Useful for rendering a global badge and per-conversation badges in the chat list.
/// </summary>
public class ChatUnreadSummaryDto
{
    public int TotalUnread { get; set; }
    public List<ChatGroupUnreadDto> Groups { get; set; } = new();
}

public class ChatGroupUnreadDto
{
    public Guid GroupId { get; set; }
    public string? GroupName { get; set; }
    public int UnreadCount { get; set; }
}
