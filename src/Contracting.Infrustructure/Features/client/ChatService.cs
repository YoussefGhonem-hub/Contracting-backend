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
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ChatService : IChatService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;
    private readonly IFirebaseService _firebase;
    private readonly Features.Firebase.FirebaseOptions _firebaseOptions;
    private readonly INotificationService _notificationService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        ApplicationDbContext db,
        IStorageService storage,
        IFirebaseService firebase,
        IOptions<Features.Firebase.FirebaseOptions> firebaseOptions,
        INotificationService notificationService,
        ILogger<ChatService> logger)
    {
        _db = db;
        _storage = storage;
        _firebase = firebase;
        _firebaseOptions = firebaseOptions.Value;
        _notificationService = notificationService;
        _logger = logger;
    }

    // -------------------------------------------------------------------------
    // Group Management
    // -------------------------------------------------------------------------

    public async Task<GetChatGroupDto?> GetOrCreateGroupAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var callerId = CurrentUser.Id;

        var group = await _db.ChatGroups
            .Include(g => g.Members).ThenInclude(m => m.ApplicationUser)
            .Include(g => g.Project)
            .FirstOrDefaultAsync(g => g.ProjectId == projectId, cancellationToken);

        if (group is not null)
        {
            // Auto-add caller as a member if they are not already in the group
            if (callerId.HasValue && !group.Members.Any(m => m.ApplicationUserId == callerId.Value))
            {
                _db.ChatGroupMembers.Add(new ChatGroupMember
                {
                    ChatGroupId = group.Id,
                    ApplicationUserId = callerId.Value,
                    MemberType = "TeamMember"
                });
                await _db.SaveChangesAsync(cancellationToken);

                // Reload so returned members list is up to date
                group = await _db.ChatGroups
                    .Include(g => g.Members).ThenInclude(m => m.ApplicationUser)
                    .Include(g => g.Project)
                    .FirstAsync(g => g.Id == group.Id, cancellationToken);
            }

            return MapGroupDto(group);
        }

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

        _db.ChatGroups.Add(group);

        // Auto-add the caller (engineer/admin creating the group) as a TeamMember
        if (callerId.HasValue)
        {
            _db.ChatGroupMembers.Add(new ChatGroupMember
            {
                ChatGroupId = group.Id,
                ApplicationUserId = callerId.Value,
                MemberType = "TeamMember"
            });
        }

        // Auto-add the client of this project as a member
        var clientUser = await _db.ClientProjects
            .Where(cp => cp.ProjectId == projectId)
            .Select(cp => new { cp.Client.ApplicationUserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (clientUser is not null && clientUser.ApplicationUserId != callerId)
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

    public async Task<List<GetChatMemberDto>?> GetMembersAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userIdNullable = CurrentUser.Id;
        if (!userIdNullable.HasValue) return null;
        var userId = userIdNullable.Value;

        var group = await _db.ChatGroups
            .Include(g => g.Members).ThenInclude(m => m.ApplicationUser)
            .AsNoTracking()
            .FirstOrDefaultAsync(g => g.Id == groupId, cancellationToken);

        if (group is null)
            return null;

        // Only members of the group may view the member list
        if (!group.Members.Any(m => m.ApplicationUserId == userId))
            return null;

        return group.Members.Select(m => new GetChatMemberDto
        {
            UserId = m.ApplicationUserId,
            FullName = m.ApplicationUser?.FullName,
            Email = m.ApplicationUser?.Email,
            AvatarUrl = m.ApplicationUser?.AvatarUrl,
            MemberType = m.MemberType
        }).ToList();
    }

    public async Task<bool> AssignMemberAsync(Guid groupId, Guid userId, string memberType = "TeamMember", CancellationToken cancellationToken = default)
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

        var resolvedType = memberType is "Client" or "TeamMember" ? memberType : "TeamMember";

        _db.ChatGroupMembers.Add(new ChatGroupMember
        {
            ChatGroupId = groupId,
            ApplicationUserId = userId,
            MemberType = resolvedType
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
        var senderIdNullable = CurrentUser.Id;
        if (!senderIdNullable.HasValue) return null;
        var senderId = senderIdNullable.Value;

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

        // Push to Firebase Firestore for real-time delivery (non-critical — don't let failures break message send)
        try
        {
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
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Firebase push failed for message {MessageId} — message was saved to DB", message.Id);
        }

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
        var senderIdNullable = CurrentUser.Id;
        if (!senderIdNullable.HasValue) return null;
        var senderId = senderIdNullable.Value;

        if (!await IsMemberAsync(groupId, senderId, cancellationToken))
            return null;

        // Determine attachment type from extension
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var isImage = ext is ".jpg" or ".jpeg" or ".png" or ".gif" or ".webp";
        var msgType = isImage ? ChatMessageType.Image : ChatMessageType.Document;

        // Upload to S3 (private bucket) — the object Key is what we persist; access URLs are
        // generated on read via pre-signed URLs so they work on AWS and across instances.
        var stored = await _storage.Upload(file, cancellationToken);

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
            Key = stored.Key,
            FileName = stored.FileName ?? file.FileName,
            Extension = stored.Extension ?? ext,
            FileSize = stored.FileSize ?? file.Length,
            Url = stored.Url,
            AttachmentType = msgType
        };

        message.Attachments.Add(attachment);
        _db.ChatMessages.Add(message);
        await _db.SaveChangesAsync(cancellationToken);

        var sender = await _db.Users.FindAsync(new object[] { senderId }, cancellationToken);

        try
        {
            await _firebase.PushMessageAsync(new Inteface.client.FirestoreChatMessage(
                MessageId: message.Id.ToString(),
                ChatGroupId: groupId.ToString(),
                SenderId: senderId.ToString(),
                SenderName: sender?.FullName ?? sender?.Email ?? "Unknown",
                Content: file.FileName,
                MessageType: msgType,
                AttachmentUrl: ResolveAttachmentUrl(attachment.Key, attachment.Url),
                AttachmentFileName: attachment.FileName,
                AttachmentFileSize: attachment.FileSize,
                SentAt: DateTimeHelper.DateTimeNow
            ), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Firebase push failed for attachment message {MessageId} — message was saved to DB", message.Id);
        }

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
                    Url = ResolveAttachmentUrl(attachment.Key, attachment.Url),
                    AttachmentType = msgType.ToString()
                }
            }
        };
    }

    public async Task<GetChatMessagesPagedDto?> GetMessagesAsync(
        Guid groupId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var userIdNullable = CurrentUser.Id;
        if (!userIdNullable.HasValue) return null;
        var userId = userIdNullable.Value;

        var member = await _db.ChatGroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);
        if (member is null) return null;

        var lastReadAt = member.LastReadAt;

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
            Messages = messages.Select(m => MapMessageDto(m, userId, lastReadAt)).ToList()
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

        var member = await _db.ChatGroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);
        if (member is null) return null;

        var lastReadAt = member.LastReadAt;

        var messages = await _db.ChatMessages
            .Where(m => m.ChatGroupId == groupId && m.MessageType == type)
            .OrderByDescending(m => m.CreatedDate)
            .Include(m => m.Sender)
            .Include(m => m.Attachments)
            .ToListAsync(cancellationToken);

        return messages.Select(m => MapMessageDto(m, userId, lastReadAt)).ToList();
    }

    // -------------------------------------------------------------------------
    // Read Receipt
    // -------------------------------------------------------------------------

    public async Task MarkMessagesReadAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userIdNullable = CurrentUser.Id;
        if (!userIdNullable.HasValue) return;
        var userId = userIdNullable.Value;

        var member = await _db.ChatGroupMembers
            .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);

        // Not a member — nothing to mark
        if (member is null) return;

        // Per-user read tracking: advance ONLY this member's read marker to "now".
        // We intentionally do NOT touch ChatMessage.IsRead — that is a single shared field and
        // flipping it would mark messages read for every member. Each member's unread count is
        // derived from their own LastReadAt, so reading affects this member only.
        member.LastReadAt = DateTimeHelper.DateTimeNow;

        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<int?> GetUnreadCountAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userIdNullable = CurrentUser.Id;
        if (!userIdNullable.HasValue) return null;
        var userId = userIdNullable.Value;

        var member = await _db.ChatGroupMembers
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.ChatGroupId == groupId && m.ApplicationUserId == userId, cancellationToken);

        // Not a member of this group
        if (member is null) return null;

        var lastReadAt = member.LastReadAt;

        return await _db.ChatMessages
            .CountAsync(m => m.ChatGroupId == groupId
                          && m.SenderId != userId
                          && (lastReadAt == null || m.CreatedDate > lastReadAt),
                        cancellationToken);
    }

    public async Task<ChatUnreadSummaryDto> GetUnreadSummaryAsync(CancellationToken cancellationToken = default)
    {
        var userIdNullable = CurrentUser.Id;
        if (!userIdNullable.HasValue)
            return new ChatUnreadSummaryDto();
        var userId = userIdNullable.Value;

        // All groups the user belongs to, with their per-user read marker and a display name.
        var memberships = await _db.ChatGroupMembers
            .AsNoTracking()
            .Where(m => m.ApplicationUserId == userId)
            .Select(m => new
            {
                m.ChatGroupId,
                m.LastReadAt,
                GroupName = m.ChatGroup.Name ?? m.ChatGroup.Project.nameEn
            })
            .ToListAsync(cancellationToken);

        // Unread counts per group in a single grouped query (groups with no unread won't appear here).
        var counts = await (
            from msg in _db.ChatMessages.AsNoTracking()
            join mem in _db.ChatGroupMembers.AsNoTracking()
                on msg.ChatGroupId equals mem.ChatGroupId
            where mem.ApplicationUserId == userId
               && msg.SenderId != userId
               && (mem.LastReadAt == null || msg.CreatedDate > mem.LastReadAt)
            group msg by msg.ChatGroupId into g
            select new { ChatGroupId = g.Key, Count = g.Count() }
        ).ToDictionaryAsync(x => x.ChatGroupId, x => x.Count, cancellationToken);

        var groups = memberships.Select(m => new ChatGroupUnreadDto
        {
            GroupId = m.ChatGroupId,
            GroupName = m.GroupName,
            UnreadCount = counts.TryGetValue(m.ChatGroupId, out var c) ? c : 0
        }).ToList();

        return new ChatUnreadSummaryDto
        {
            TotalUnread = groups.Sum(g => g.UnreadCount),
            Groups = groups
        };
    }

    // -------------------------------------------------------------------------
    // Firebase Token
    // -------------------------------------------------------------------------

    public async Task<FirebaseTokenDto?> GetFirebaseTokenAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        var userIdNullable = CurrentUser.Id;
        if (!userIdNullable.HasValue) return null;
        var userId = userIdNullable.Value;

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

    /// <summary>
    /// Resolves the URL clients should use to fetch an attachment. The S3 bucket is private, so we
    /// return a time-limited pre-signed URL generated from the stored object Key. Falls back to the
    /// stored Url when no Key is available (e.g. legacy local-disk records).
    /// </summary>
    private string? ResolveAttachmentUrl(string? key, string? storedUrl)
        => (string.IsNullOrWhiteSpace(key) ? null : _storage.GetPreSignedUrl(key)) ?? storedUrl;

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

    /// <summary>
    /// Maps a message for a specific viewer. IsRead is computed per-user: a message is "read" by the
    /// viewer if they sent it, or if it was created on/before the viewer's own LastReadAt marker.
    /// </summary>
    private GetChatMessageDto MapMessageDto(ChatMessage m, Guid viewerId, DateTimeOffset? viewerLastReadAt) => new()
    {
        Id = m.Id,
        SenderId = m.SenderId,
        SenderName = m.Sender?.FullName ?? m.Sender?.Email,
        SenderAvatarUrl = m.Sender?.AvatarUrl,
        Content = m.Content,
        MessageType = m.MessageType.ToString(),
        IsRead = m.SenderId == viewerId
                 || (viewerLastReadAt != null && m.CreatedDate <= viewerLastReadAt),
        SentAt = m.CreatedDate,
        Attachments = m.Attachments.Select(a => new GetChatAttachmentDto
        {
            Id = a.Id,
            FileName = a.FileName,
            Extension = a.Extension,
            FileSize = a.FileSize,
            Url = ResolveAttachmentUrl(a.Key, a.Url),
            AttachmentType = a.AttachmentType.ToString()
        }).ToList()
    };
}
