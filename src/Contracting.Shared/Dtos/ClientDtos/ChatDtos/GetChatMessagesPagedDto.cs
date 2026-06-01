namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

public class GetChatMessagesPagedDto
{
    public List<GetChatMessageDto> Messages { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public bool HasMore { get; set; }
}
