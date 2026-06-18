using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ChatDtos;
using Contracting.Shared.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Contracting.Infrustructure.Features.client;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly IFirebaseService _firebase;
    private readonly Features.Firebase.FirebaseOptions _firebaseOptions;
    private readonly INotificationService _notificationService;

    public ChatService(
        ApplicationDbContext db,
        IFileStorage fileStorage,
        IFirebaseService firebase,
        IOptions<Features.Firebase.FirebaseOptions> firebaseOptions,
        INotificationService notificationService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _firebase = firebase;
        _firebaseOptions = firebaseOptions.Value;
        _notificationService = notificationService;
    }

    // -------------------------------------------------------------------------
    // Group Management
    // -------------------------------------------------------------------------

    public async Task<GetChatGroupDto?> GetOrCreateGroupAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var group = await _db.ChatGroups
            .Include(g => g.Members).ThenInclude(m => m.ApplicationUser)
            .Include(g => g.Project)
            .FirstOrDefaultAsync(g => g.ProjectId == projectId, cancellationToken);

        if (group is not null)
            return MapGroupDto(group);

        // Auto-create the chat group for this project
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId, cancellationToken);
        if (project is null)
            return null;

        group = new ChatGroup
        {
            ProjectId = projectId,
            Name = $"{project.nameEn} Chat"
        };

        // Auto-add the client of this project as a member
        var clientUser = await _db.ClientProjects
            .Where(cp => cp.ProjectId == projectId)
            .Select(cp => new { cp.Client.ApplicationUserId })
            .FirstOrDefaultAsync(cancellationToken);

        _db.ChatGroups.Add(group);

        if (clientUser is not null)
        {
            _db.ChatGroupMembers.Add(new ChatGroupMember
            {
                ChatGroupId = group.Id,
                ApplicationUserId = clientUser.ApplicationUserId,
                MemberType = "Client"
            });
        }

        await _db.SaveChangesAsync(cancellationToken);

        // Reload with navigation props
        group = await _db.ChatGroups
            .Include(g => g.Members).ThenInclude(m => m.ApplicationUser)
            .Include(g => g.Project)
            .FirstAsync(g => g.Id == group.Id, cancellationToken);

        return MapGroupDto(group);
    }

    public async Task<GetChatGroupDto?> GetGroupAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var group = await _db.ChatGroups
            .Include(g => g.Members).ThenInclude(m => m.ApplicationUser)
            .Include(g => g.Project)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group is null)
            return null;

        // Client can only see their own group; admin/team can see any group they're a member of
        var isMember = group.Members.Any(m => m.ApplicationUserId == userId);
        if (!isMember)
            return null;

        return MapGroupDto(group);
    }

    // -------------------------------------------------------------------------
    // Member Management
    // -------------------------------------------------------------------------

    public async Task<bool> AssignMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var group = await _db.ChatGroups
            .Include(g => g.Project)
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);
        if (group is null) return false;

        var alreadyMember = await _db.ChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);
        if (alreadyMember) return true;

        var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken);
        if (user is null) return false;

        _db.ChatGroupMembers.Add(new ChatGroupMember
        {
            ChatGroupId = groupId,
            ApplicationUserId = userId,
            MemberType = "TeamMember"
        });

        await _db.SaveChangesAsync(cancellationToken);

        var groupName = group.Name ?? group.Project?.nameEn ?? "Chat Group";
        await _notificationService.SendNotificationToUserAsync(
            userId,
            "Added to Chat Group",
            $"You have been added to the chat group: {groupName}");

        return true;
    }

    public async Task<bool> RemoveMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken = default)
    {
        var member = await _db.ChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);

        if (member is null) return false;

        // Protect the client from being removed
        if (member.MemberType == "Client") return false;

        _db.ChatGroupMembers.Remove(member);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }

    // -------------------------------------------------------------------------
    // Messaging
    // -------------------------------------------------------------------------

    public async Task<GetChatMessageDto?> SendTextMessageAsync(
        Guid groupId, string content, string messageTypeStr, CancellationToken cancellationToken = default)
    {
        var senderId = CurrentUser.Id!.Value;

        if (!await IsMemberAsync(groupId, senderId, cancellationToken))
            return null;

        if (!Enum.TryParse<ChatMessageType>(messageTypeStr, ignoreCase: true, out var msgType))
            msgType = ChatMessageType.Text;

        if (msgType == ChatMessageType.Image || msgType == ChatMessageType.Document)
            msgType = ChatMessageType.Text;

        var message = new ChatMessage
        {
            ChatGroupId = groupId,
            SenderId = senderId,
            Content = content,
            MessageType = msgType,
            IsRead = false
        };

        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var sender = await _db.Users.FindAsync(new object[] { senderId }, cancellationToken);

        // Push to Firebase Firestore for real-time delivery
        await _firebase.PushMessageAsync(new Inteface.client.FirestoreChatMessage(
            MessageId: message.Id.ToString(),
            ChatGroupId: groupId.ToString(),
            SenderId: senderId.ToString(),
            SenderName: sender?.FullName ?? sender?.Email ?? "Unknown",
            Content: content,
            MessageType: msgType,
            AttachmentUrl: null,
            AttachmentFileName: null,
            AttachmentFileSize: null,
            SentAt: DateTimeHelper.DateTimeNow
        ), cancellationToken);

        var senderName = sender?.FullName ?? sender?.Email ?? "Someone";
        var preview = content.Length > 80 ? content[..80] + "…" : content;
        await NotifyGroupMembersAsync(groupId, senderId, senderName, preview, cancellationToken);

        return new GetChatMessageDto
        {
            Id = message.Id,
            SenderId = senderId,
            SenderName = sender?.FullName ?? sender?.Email,
            SenderAvatarUrl = sender?.AvatarUrl,
            Content = content,
            MessageType = msgType.ToString(),
            IsRead = false,
            SentAt = message.CreatedDate,
            Attachments = new List<GetChatAttachmentDto>()
        };
    }

    public async Task<GetChatMessageDto?> SendAttachmentMessageAsync(
        Guid groupId, IFormFile file, CancellationToken cancellationToken = default)
    {
        var senderId = CurrentUser.Id!.Value;

        if (!await IsMemberAsync(groupId, senderId, cancellationToken))
            return null;

        // Determine attachment type from extension
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isImage = ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        var msgType = isImage ? ChatMessageType.Image : ChatMessageType.Document;

        var savedPath = await _fileStorage.SaveAsync(file, "uploads/chat", cancellationToken);
        var fileName = Path.GetFileName(savedPath);

        var message = new ChatMessage
        {
            ChatGroupId = groupId,
            SenderId = senderId,
            Content = file.FileName,
            MessageType = msgType,
            IsRead = false
        };

        var attachment = new ChatMessageAttachment
        {
            ChatMessageId = message.Id,
            Key = savedPath,
            FileName = file.FileName,
            Extension = ext,
            FileSize = file.Length,
            Url = $"/uploads/chat/{fileName}",
            AttachmentType = msgType
        };

        message.Attachments.Add(attachment);
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var sender = await _db.Users.FindAsync(new object[] { senderId }, cancellationToken);

        await _firebase.PushMessageAsync(new Inteface.client.FirestoreChatMessage(
            MessageId: message.Id.ToString(),
            ChatGroupId: groupId.ToString(),
            SenderId: senderId.ToString(),
            SenderName: sender?.FullName ?? sender?.Email ?? "Unknown",
            Content: file.FileName,
            MessageType: msgType,
            AttachmentUrl: attachment.Url,
            AttachmentFileName: attachment.FileName,
            AttachmentFileSize: attachment.FileSize,
            SentAt: DateTimeHelper.DateTimeNow
        ), cancellationToken);

        var senderName = sender?.FullName ?? sender?.Email ?? "Someone";
        var preview = msgType == ChatMessageType.Image ? "📷 Sent an image" : $"📎 {file.FileName}";
        await NotifyGroupMembersAsync(groupId, senderId, senderName, preview, cancellationToken);

        return new GetChatMessageDto
        {
            Id = message.Id,
            SenderId = senderId,
            SenderName = sender?.FullName ?? sender?.Email,
            SenderAvatarUrl = sender?.AvatarUrl,
            Content = file.FileName,
            MessageType = msgType.ToString(),
            IsRead = false,
            SentAt = message.CreatedDate,
            Attachments = new List<GetChatAttachmentDto>
            {
                new GetChatAttachmentDto
                {
                    Id = attachment.Id,
                    FileName = attachment.FileName,
                    Extension = attachment.Extension,
                    FileSize = attachment.FileSize,
                    Url = attachment.Url,
                    AttachmentType = msgType.ToString()
                }
            }
        };
    }

    public async Task<GetChatMessagesPagedDto?> GetMessagesAsync(
        Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        if (!await IsMemberAsync(groupId, userId, cancellationToken))
            return null;

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _db.ChatMessages
            .CountAsync(m => m.ChatGroupId == groupId, cancellationToken);

        var messages = await _db.ChatMessages
            .Where(m => m.ChatGroupId == groupId)
            .OrderByDescending(m => m.CreatedDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .ToListAsync(cancellationToken);

        return new GetChatMessagesPagedDto
        {
            Page = page,
            PageSize = pageSize,
            TotalCount = total,
            HasMore = (page * pageSize) < total,
            Messages = messages.Select(MapMessageDto).ToList()
        };
    }

    // -------------------------------------------------------------------------
    // Tab Filters
    // -------------------------------------------------------------------------

    public async Task<List<GetChatMessageDto>?> GetMediaMessagesAsync(
        Guid groupId, CancellationToken cancellationToken = default)
    {
        return await GetMessagesByTypeAsync(groupId, ChatMessageType.Image, cancellationToken);
    }

    public async Task<List<GetChatMessageDto>?> GetDocumentMessagesAsync(
        Guid groupId, CancellationToken cancellationToken = default)
    {
        return await GetMessagesByTypeAsync(groupId, ChatMessageType.Document, cancellationToken);
    }

    public async Task<List<GetChatMessageDto>?> GetLinkMessagesAsync(
        Guid groupId, CancellationToken cancellationToken = default)
    {
        return await GetMessagesByTypeAsync(groupId, ChatMessageType.Link, cancellationToken);
    }

    private async Task<List<GetChatMessageDto>?> GetMessagesByTypeAsync(
        Guid groupId, ChatMessageType type, CancellationToken cancellationToken)
    {
        var userId = CurrentUser.Id!.Value;

        if (!await IsMemberAsync(groupId, userId, cancellationToken))
            return null;

        var messages = await _db.ChatMessages
            .Where(m => m.ChatGroupId == groupId && m.MessageType == type)
            .OrderByDescending(m => m.CreatedDate)
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .ToListAsync(cancellationToken);

        return messages.Select(MapMessageDto).ToList();
    }

    // -------------------------------------------------------------------------
    // Read Receipt
    // -------------------------------------------------------------------------

    public async Task MarkMessagesReadAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var unread = await _db.ChatMessages
            .Where(m => m.ChatGroupId == groupId && !m.IsRead && m.SenderId != userId)
            .ToListAsync(cancellationToken);

        foreach (var msg in unread)
            msg.IsRead = true;

        if (unread.Count > 0)
            await _db.SaveChangesAsync(cancellationToken);
    }

    // -------------------------------------------------------------------------
    // Firebase Token
    // -------------------------------------------------------------------------

    public async Task<FirebaseTokenDto?> GetFirebaseTokenAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        if (!await IsMemberAsync(groupId, userId, cancellationToken))
            return null;

        var token = await _firebase.GenerateCustomTokenAsync(userId.ToString(), cancellationToken);

        return new FirebaseTokenDto
        {
            Token = token,
            ProjectId = _firebaseOptions.ProjectId ?? string.Empty,
            ChatGroupPath = $"chats/{groupId}/messages"
        };
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private async Task NotifyGroupMembersAsync(
        Guid groupId, Guid senderId, string senderName, string messagePreview, CancellationToken cancellationToken)
    {
        var group = await _db.ChatGroups
            .Include(g => g.Members)
            .Include(g => g.Project)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group is null) return;

        var groupName = group.Name ?? group.Project?.nameEn ?? "Chat Group";
        var otherMembers = group.Members
            .Where(m => m.ApplicationUserId != senderId)
            .Select(m => m.ApplicationUserId)
            .ToList();

        var tasks = otherMembers.Select(userId =>
            _notificationService.SendNotificationToUserAsync(
                userId,
                groupName,
                $"{senderName}: {messagePreview}"));

        await Task.WhenAll(tasks);
    }

    private async Task<bool> IsMemberAsync(Guid groupId, Guid userId, CancellationToken cancellationToken)
    {
        return await _db.ChatGroupMembers
            .AnyAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);
    }

    private static GetChatGroupDto MapGroupDto(ChatGroup group) => new()
    {
        Id = group.Id,
        ProjectId = group.ProjectId,
        ProjectName = group.Project?.nameEn,
        Name = group.Name,
        Members = group.Members.Select(m => new GetChatMemberDto
        {
            UserId = m.ApplicationUserId,
            FullName = m.ApplicationUser?.FullName,
            Email = m.ApplicationUser?.Email,
            AvatarUrl = m.ApplicationUser?.AvatarUrl,
            MemberType = m.MemberType
        }).ToList()
    };

    private static GetChatMessageDto MapMessageDto(ChatMessage m) => new()
    {
        Id = m.Id,
        SenderId = m.SenderId,
        SenderName = m.Sender?.FullName ?? m.Sender?.Email,
        SenderAvatarUrl = m.Sender?.AvatarUrl,
        Content = m.Content,
        MessageType = m.MessageType.ToString(),
        IsRead = m.IsRead,
        SentAt = m.CreatedDate,
        Attachments = m.Attachments.Select(a => new GetChatAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            Extension = a.Extension,
            FileSize = a.FileSize,
            Url = a.Url,
            AttachmentType = a.AttachmentType.ToString()
        }).ToList()
    };
}
