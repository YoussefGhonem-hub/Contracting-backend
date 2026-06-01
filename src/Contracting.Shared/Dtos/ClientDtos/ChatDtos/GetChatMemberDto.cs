namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

public class GetChatMemberDto
{
    public Guid UserId { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
    public string? MemberType { get; set; }
}
