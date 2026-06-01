using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using Microsoft.AspNetCore.Http;

namespace Contracting.Infrustructure.Inteface.client;

public interface IChatService
{
    // --- Group Management ---
    Task<GetChatGroupDto?> GetOrCreateGroupAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<GetChatGroupDto?> GetGroupAsync(Guid groupId, CancellationToken cancellationToken = default);

    // --- Member Management ---
    Task<bool> AssignMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);
    Task<bool> RemoveMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default);

    // --- Messaging ---
    Task<GetChatMessageDto?> SendTextMessageAsync(Guid groupId, string content, string messageType, CancellationToken cancellationToken = default);
    Task<GetChatMessageDto?> SendAttachmentMessageAsync(Guid groupId, IFormFile file, CancellationToken cancellationToken = default);
    Task<GetChatMessagesPagedDto?> GetMessagesAsync(Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default);

    // --- Tab Filters ---
    Task<List<GetChatMessageDto>?> GetMediaMessagesAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<List<GetChatMessageDto>?> GetDocumentMessagesAsync(Guid groupId, CancellationToken cancellationToken = default);
    Task<List<GetChatMessageDto>?> GetLinkMessagesAsync(Guid groupId, CancellationToken cancellationToken = default);

    // --- Read Receipt ---
    Task MarkMessagesReadAsync(Guid groupId, CancellationToken cancellationToken = default);

    // --- Firebase Token ---
    Task<FirebaseTokenDto?> GetFirebaseTokenAsync(Guid groupId, CancellationToken cancellationToken = default);
}
