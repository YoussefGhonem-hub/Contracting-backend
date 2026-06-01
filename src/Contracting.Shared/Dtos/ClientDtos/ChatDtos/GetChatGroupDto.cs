namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

public class GetChatGroupDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? ProjectName { get; set; }
    public string? Name { get; set; }
    public List<GetChatMemberDto> Members { get; set; } = new();
}
