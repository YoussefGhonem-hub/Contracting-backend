namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

/// <summary>
/// Request body for sending a text or link message.
/// </summary>
public class SendTextMessageDto
{
    public string Content { get; set; } = string.Empty;
    /// <summary>
    /// "Text" or "Link". Defaults to "Text".
    /// </summary>
    public string MessageType { get; set; } = "Text";
}
