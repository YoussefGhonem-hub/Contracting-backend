using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.Chat.Command.AssignChatMember;
using Contracting.Application.Features.Client.Chat.Command.MarkMessagesRead;
using Contracting.Application.Features.Client.Chat.Command.RemoveChatMember;
using Contracting.Application.Features.Client.Chat.Command.SendAttachment;
using Contracting.Application.Features.Client.Chat.Command.SendTextMessage;
using Contracting.Application.Features.Client.Chat.Query.GetChatGroup;
using Contracting.Application.Features.Client.Chat.Query.GetChatMessages;
using Contracting.Application.Features.Client.Chat.Query.GetChatTabMedia;
using Contracting.Application.Features.Client.Chat.Query.GetFirebaseToken;
using Contracting.Application.Features.Client.Chat.Query.GetOrCreateChatGroup;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

/// <summary>
/// Chat Module — unified for Mobile + Web clients.
/// Roles: Client can access their own project chat.
/// Admin / Team members access chats they are assigned to.
/// </summary>
[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatController : APIBaseController
{
    private readonly IMediator _mediator;

    public ChatController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // =========================================================================
    // Group Management
    // =========================================================================

    /// <summary>
    /// Get or create the chat group for a specific project.
    /// Creates the group automatically if it does not exist yet.
    /// Accessible by Admin, Teamlead-engineer, or any authenticated team member.
    /// </summary>
    [HttpPost("projects/{projectId:guid}/group")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.SuperAdmin},{RoleNames.Teamleadengineer}")]
    public async Task<IActionResult> GetOrCreateGroup(Guid projectId)
    {
        var query = new GetOrCreateChatGroupQuery(projectId);
        var result = await _mediator.Send(query);
        return result.Match(g => Ok(g), errors => Problem(errors));
    }

    /// <summary>
    /// Get the chat group details (info + members list) by group ID.
    /// Only members of the group can access this endpoint.
    /// </summary>
    [HttpGet("groups/{groupId:guid}")]
    public async Task<IActionResult> GetGroup(Guid groupId)
    {
        var query = new GetChatGroupQuery(groupId);
        var result = await _mediator.Send(query);
        return result.Match(g => Ok(g), errors => Problem(errors));
    }

    // =========================================================================
    // Member Management (Admin only)
    // =========================================================================

    /// <summary>
    /// Assign a team member (by userId) to a chat group so they can reply to the client.
    /// </summary>
    [HttpPost("groups/{groupId:guid}/members")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.SuperAdmin},{RoleNames.Teamleadengineer}")]
    public async Task<IActionResult> AssignMember(Guid groupId, [FromBody] AssignChatMemberDto dto)
    {
        var command = new AssignChatMemberCommand(groupId, dto.UserId);
        var result = await _mediator.Send(command);
        return result.Match(_ => Ok(new { message = "Member assigned successfully." }), errors => Problem(errors));
    }

    /// <summary>
    /// Remove a team member from a chat group.
    /// The client user cannot be removed.
    /// </summary>
    [HttpDelete("groups/{groupId:guid}/members/{userId:guid}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.SuperAdmin},{RoleNames.Teamleadengineer}")]
    public async Task<IActionResult> RemoveMember(Guid groupId, Guid userId)
    {
        var command = new RemoveChatMemberCommand(groupId, userId);
        var result = await _mediator.Send(command);
        return result.Match(_ => Ok(new { message = "Member removed successfully." }), errors => Problem(errors));
    }

    // =========================================================================
    // Messaging
    // =========================================================================

    /// <summary>
    /// Get paginated message history for a chat group.
    /// Returns messages in reverse-chronological order (newest first).
    /// Query: ?page=1&amp;pageSize=30
    /// </summary>
    [HttpGet("groups/{groupId:guid}/messages")]
    public async Task<IActionResult> GetMessages(
        Guid groupId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var query = new GetChatMessagesQuery(groupId, page, pageSize);
        var result = await _mediator.Send(query);
        return result.Match(m => Ok(m), errors => Problem(errors));
    }

    /// <summary>
    /// Send a text message or a link message to the chat group.
    /// MessageType: "Text" | "Link"
    /// The message is saved to the database AND pushed to Firebase Firestore for real-time delivery.
    /// </summary>
    [HttpPost("groups/{groupId:guid}/messages")]
    public async Task<IActionResult> SendTextMessage(Guid groupId, [FromBody] SendTextMessageDto dto)
    {
        var command = new SendTextMessageCommand(groupId, dto.Content, dto.MessageType);
        var result = await _mediator.Send(command);
        return result.Match(m => Ok(m), errors => Problem(errors));
    }

    /// <summary>
    /// Send a file attachment (image or document) to the chat group.
    /// The file type is auto-detected from the extension:
    /// - .jpg, .jpeg, .png, .gif, .webp → Image (appears in Media tab)
    /// - everything else → Document (appears in Docs tab)
    /// Use multipart/form-data with field name "file".
    /// </summary>
    [HttpPost("groups/{groupId:guid}/messages/attachment")]
    public async Task<IActionResult> SendAttachment(Guid groupId, IFormFile file)
    {
        var command = new SendAttachmentCommand(groupId, file);
        var result = await _mediator.Send(command);
        return result.Match(m => Ok(m), errors => Problem(errors));
    }

    // =========================================================================
    // Tab Filters: Docs / Links / Media
    // =========================================================================

    /// <summary>
    /// Get all media messages (images) in the chat group. Used by the "Media" tab.
    /// </summary>
    [HttpGet("groups/{groupId:guid}/media")]
    public async Task<IActionResult> GetMediaTab(Guid groupId)
    {
        var query = new GetChatMediaQuery(groupId, "media");
        var result = await _mediator.Send(query);
        return result.Match(m => Ok(m), errors => Problem(errors));
    }

    /// <summary>
    /// Get all document messages in the chat group. Used by the "Docs" tab.
    /// </summary>
    [HttpGet("groups/{groupId:guid}/docs")]
    public async Task<IActionResult> GetDocsTab(Guid groupId)
    {
        var query = new GetChatMediaQuery(groupId, "docs");
        var result = await _mediator.Send(query);
        return result.Match(m => Ok(m), errors => Problem(errors));
    }

    /// <summary>
    /// Get all link messages in the chat group. Used by the "Links" tab.
    /// </summary>
    [HttpGet("groups/{groupId:guid}/links")]
    public async Task<IActionResult> GetLinksTab(Guid groupId)
    {
        var query = new GetChatMediaQuery(groupId, "links");
        var result = await _mediator.Send(query);
        return result.Match(m => Ok(m), errors => Problem(errors));
    }

    // =========================================================================
    // Read Receipt
    // =========================================================================

    /// <summary>
    /// Mark all unread messages in a chat group as read for the current user.
    /// </summary>
    [HttpPut("groups/{groupId:guid}/read")]
    public async Task<IActionResult> MarkAsRead(Guid groupId)
    {
        var command = new MarkMessagesReadCommand(groupId);
        var result = await _mediator.Send(command);
        return result.Match(_ => Ok(new { message = "Messages marked as read." }), errors => Problem(errors));
    }

    // =========================================================================
    // Firebase Token (Real-time Auth)
    // =========================================================================

    /// <summary>
    /// Get a Firebase custom token for the current user.
    /// The client app must use this token to authenticate with Firebase and subscribe
    /// to the Firestore collection at the returned "chatGroupPath" for real-time messages.
    /// Token expires in 1 hour — refresh as needed.
    /// </summary>
    [HttpGet("groups/{groupId:guid}/firebase-token")]
    public async Task<IActionResult> GetFirebaseToken(Guid groupId)
    {
        var query = new GetFirebaseTokenQuery(groupId);
        var result = await _mediator.Send(query);
        return result.Match(t => Ok(t), errors => Problem(errors));
    }
}
