namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

/// <summary>
/// Request body for assigning a team member to a chat group.
/// </summary>
public class AssignChatMemberDto
{
    public Guid UserId { get; set; }
}
