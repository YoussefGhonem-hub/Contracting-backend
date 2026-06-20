namespace Contracting.Shared.Dtos.ClientDtos.ChatDtos;

/// <summary>
/// Request body for assigning a member to a chat group.
/// MemberType: "TeamMember" (engineers/admins) | "Client"
/// </summary>
public class AssignChatMemberDto
{
    public Guid UserId { get; set; }
    public string MemberType { get; set; } = "TeamMember";
}
