using Contracting.Domain.Entities.helper;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.HelperDtos;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using MapsterMapper;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Contracting.Infrustructure.Features.Helper
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly IMapper _mapper;

        public NotificationService(ILogger<NotificationService> logger, ApplicationDbContext db, IOptions<FirebaseSettings> firebaseOptions, IBackgroundJobClient backgroundJobClient, IMapper mapper)
        {
            _logger = logger;
            _mapper = mapper;

            if (FirebaseApp.DefaultInstance == null)
            {
                var settings = firebaseOptions.Value;
                var json = $@"{{
                ""type"": ""{settings.Type}"",
                ""project_id"": ""{settings.ProjectId}"",
                ""private_key_id"": ""{settings.PrivateKeyId}"",
                ""private_key"": ""{settings.PrivateKey}"",
                ""client_email"": ""{settings.ClientEmail}"",
                ""client_id"": ""{settings.ClientId}"",
                ""auth_uri"": ""{settings.AuthUri}"",
                ""token_uri"": ""{settings.TokenUri}"",
                ""auth_provider_x509_cert_url"": ""{settings.AuthProviderX509CertUrl}"",
                ""client_x509_cert_url"": ""{settings.ClientX509CertUrl}"",
                ""universe_domain"": ""{settings.UniverseDomain}""
            }}";
                FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(json)
                });
            }

            _db = db;
            _backgroundJobClient = backgroundJobClient;
        }

        public async Task<bool> GenerateToken(string fcmToken)
        {
            var currentUserId = Guid.Parse(CurrentUser.UserId);

            // A device push token uniquely identifies a single physical device, so it must belong
            // to exactly one user — the one currently logged in on it. Remove any registrations of
            // the same token under OTHER users (e.g. a previous account that logged in on this
            // device and never cleaned up). Otherwise messages meant for that previous account get
            // pushed to this device, which shows up as "I received a notification for my own message".
            var staleTokens = await _db.userDeviceTokens
                .Where(t => t.FcmToken == fcmToken && t.UserId != currentUserId)
                .ToListAsync();

            if (staleTokens.Count > 0)
                _db.userDeviceTokens.RemoveRange(staleTokens);

            var exists = await _db.userDeviceTokens
                        .AnyAsync(t => t.UserId == currentUserId
                            && t.FcmToken == fcmToken);

            if (!exists)
            {
                _db.userDeviceTokens.Add(new UserDeviceToken
                {
                    UserId = currentUserId,
                    FcmToken = fcmToken
                });
            }

            if (staleTokens.Count > 0 || !exists)
            {
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task<bool> RemoveToken(string token)
        {
            var entity = _db.userDeviceTokens
                        .FirstOrDefault(t => t.UserId == Guid.Parse(CurrentUser.UserId)
                            && t.FcmToken == token);
            if (entity != null)
            {
                _db.userDeviceTokens.Remove(entity);
                await _db.SaveChangesAsync();
                return true;
            }
            return false;
        }

        public async Task SendAsync(PushNotificationDto notification)
        {
            var data = notification.Data ?? new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(notification.EngineerId))
                data["EngineerId"] = notification.EngineerId;
            if (!string.IsNullOrEmpty(notification.DepartmentId))
                data["DepartmentId"] = notification.DepartmentId;
            if (!string.IsNullOrEmpty(notification.RequestId))
                data["RequestId"] = notification.RequestId;
            if (!string.IsNullOrEmpty(notification.ChatGroupId))
                data["ChatGroupId"] = notification.ChatGroupId;
            if (!string.IsNullOrEmpty(notification.Type))
                data["Type"] = notification.Type;

            var message = new Message()
            {
                Token = notification.Token,
                Notification = new Notification
                {
                    Title = notification.Title,
                    Body = notification.Body
                },
                Data = data
            };

            var isSent = false;
            var errorMessage = (string?)null;

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
                isSent = true;
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
                _logger.LogError(ex, "Failed to send Firebase notification");
            }
            finally
            {
                // Log the notification attempt
                await LogNotificationAsync(notification, isSent, errorMessage);
            }
        }

        private async Task LogNotificationAsync(PushNotificationDto notification, bool isSent, string? errorMessage)
        {
            try
            {
                // A single logical notification can fan out to several FCM tokens when the recipient
                // has more than one registered device. All of those per-token delivery attempts are
                // folded into the ONE NotificationLog row created up-front in
                // SendNotificationToUserAsync (identified by NotificationLogId) — otherwise every
                // device/token the user owns (including stale, no-longer-valid tokens) would produce
                // its own row and the same message would appear duplicated in the "my notifications" list.
                if (notification.NotificationLogId.HasValue)
                {
                    var existing = await _db.NotificationLogs.FindAsync(notification.NotificationLogId.Value);
                    if (existing is not null)
                    {
                        existing.Token = notification.Token;
                        // Consider the logical notification "sent" as soon as any one device receives it;
                        // don't let a later failing token flip a prior success back to failed.
                        existing.IsSent = existing.IsSent || isSent;
                        existing.ErrorMessage = isSent ? null : errorMessage;
                        await _db.SaveChangesAsync();
                        return;
                    }
                }

                var engineerId = string.IsNullOrEmpty(notification.EngineerId) ? (Guid?)null : Guid.Parse(notification.EngineerId);
                var departmentId = string.IsNullOrEmpty(notification.DepartmentId) ? (Guid?)null : Guid.Parse(notification.DepartmentId);
                var requestId = string.IsNullOrEmpty(notification.RequestId) ? (Guid?)null : Guid.Parse(notification.RequestId);
                var chatGroupId = string.IsNullOrEmpty(notification.ChatGroupId) ? (Guid?)null : Guid.Parse(notification.ChatGroupId);

                // Attribute the log to the recipient. CurrentUser is empty inside the Hangfire
                // background job (no HTTP context), so rely on the recipient id carried on the DTO.
                var logUserId = notification.UserId
                    ?? (Guid.TryParse(CurrentUser.UserId, out var current) ? current : Guid.Empty);

                var notificationLog = new NotificationLog
                {
                    UserId = logUserId,
                    Token = notification.Token,
                    Title = notification.Title,
                    Body = notification.Body,
                    IsSent = isSent,
                    ErrorMessage = errorMessage,
                    EngineerId = engineerId,
                    DepartmentId = departmentId,
                    RequestId = requestId,
                    ChatGroupId = chatGroupId,
                    Type = notification.Type,
                    SentAt = Contracting.Shared.Common.DateTimeHelper.Now
                };

                _db.NotificationLogs.Add(notificationLog);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log notification");
            }
        }

        public async Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null, Guid? chatGroupId = null, string? type = null)
        {
            if (userId == Guid.Empty)
                return;

            // Never notify the user who triggered the action about their own action.
            if (Guid.TryParse(CurrentUser.UserId, out var actingUserId) && actingUserId == userId)
                return;

            // Resolve the EngineerId from the ApplicationUserId
            var engineerId = await _db.Engineers
                .Where(e => e.ApplicationUserId == userId)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync();

            var tokens = await _db.userDeviceTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.FcmToken)
                .Distinct()
                .ToListAsync();

            // Defensive: never push to a token that is also registered to the acting user — that
            // token is physically the sender's device, so pushing to it would notify the sender of
            // their own action. (GenerateToken keeps tokens single-owner; this guards stale data.)
            if (actingUserId != Guid.Empty)
            {
                var actingUserTokens = await _db.userDeviceTokens
                    .Where(t => t.UserId == actingUserId)
                    .Select(t => t.FcmToken)
                    .ToListAsync();

                if (actingUserTokens.Count > 0)
                    tokens = tokens.Where(t => !actingUserTokens.Contains(t)).ToList();
            }

            // Create exactly ONE NotificationLog row for this logical notification, regardless of how
            // many device tokens the recipient has registered. A recipient with two (or more) devices —
            // or a stale/rotated token that was never cleaned up alongside a current one — must still
            // see the message ONCE in "my notifications", not once per token. Each per-token push
            // attempt below folds its result (sent/failed) into this same row instead of inserting its
            // own (see LogNotificationAsync).
            //
            // This row must exist even when the recipient has NO registered device (tokens.Count == 0):
            // the "my notifications" list is this row's reader, and a user who simply hasn't opened the
            // mobile app yet (or is web-only) still needs to see they were notified - a push failure is
            // not a reason to make the notification invisible everywhere.
            var notificationLog = new NotificationLog
            {
                UserId = userId,
                Title = title,
                Body = body,
                IsSent = false,
                EngineerId = engineerId,
                DepartmentId = departmentId,
                RequestId = requestId,
                ChatGroupId = chatGroupId,
                Type = type,
                SentAt = Contracting.Shared.Common.DateTimeHelper.Now
            };
            _db.NotificationLogs.Add(notificationLog);
            await _db.SaveChangesAsync();

            if (tokens.Count == 0)
                return;

            // enqueue a background job per token
            foreach (var token in tokens)
            {
                var notification = new PushNotificationDto
                {
                    Token = token,
                    UserId = userId,
                    Title = title,
                    Body = body,
                    EngineerId = engineerId?.ToString(),
                    RequestId = requestId?.ToString(),
                    DepartmentId = departmentId?.ToString(),
                    ChatGroupId = chatGroupId?.ToString(),
                    Type = type,
                    NotificationLogId = notificationLog.Id
                };
                try
                {
                    // SendAsync (the background job) is the single source of truth for recording the
                    // real send result. It updates the shared NotificationLog row created above rather
                    // than inserting a new one — do NOT log here too, or every device/token would be
                    // written to NotificationLogs as its own row and show up duplicated in the list.
                    _backgroundJobClient.Enqueue<NotificationService>(svc => svc.SendAsync(notification));
                }
                catch (Exception ex)
                {
                    // Enqueue itself failed, so SendAsync will never run — log the failure here.
                    _logger.LogError(ex, "Failed to enqueue notification job");
                    await LogNotificationAsync(notification, false, ex.Message);
                }
            }
        }

        public async Task<PaginatedList<GetNotificationDto>> GetNotificationsByEngineerAsync(Guid engineerId, NotificationFilterDto filter)
        {
            var query = _db.NotificationLogs
                .Include(n => n.Engineer)
                .Where(n => n.EngineerId == engineerId)
                .AsNoTracking();

            if (filter.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == filter.IsRead.Value);
            }

            query = query.OrderByDescending(n => n.CreatedDate);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new PaginatedList<GetNotificationDto>(
                    new List<GetNotificationDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var notifications = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var requestIds = notifications
                .Where(n => n.RequestId.HasValue)
                .Select(n => n.RequestId!.Value)
                .Distinct()
                .ToList();

            var requestTitles = requestIds.Any()
                ? await _db.EngineerRequests
                    .Where(r => requestIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.RequestTitle })
                    .ToDictionaryAsync(r => r.Id, r => r.RequestTitle)
                : new Dictionary<Guid, string?>();

            var dtos = notifications.Select(n =>
            {
                var dto = _mapper.Map<GetNotificationDto>(n);
                dto.EngineerName = n.Engineer != null
                    ? $"{n.Engineer.nameEn} / {n.Engineer.nameAr}"
                    : null;
                dto.RequestTitle = n.RequestId.HasValue && requestTitles.TryGetValue(n.RequestId.Value, out var title)
                    ? title
                    : null;
                return dto;
            }).ToList();

            return new PaginatedList<GetNotificationDto>(
                dtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<GetNotificationDto> GetNotificationByIdAsync(Guid notificationId)
        {
            var notification = await _db.NotificationLogs
                .Include(n => n.Engineer)
                .AsNoTracking()
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification is null)
                return null!;

            var dto = _mapper.Map<GetNotificationDto>(notification);
            dto.EngineerName = notification.Engineer != null
                ? $"{notification.Engineer.nameEn} / {notification.Engineer.nameAr}"
                : null;

            if (notification.RequestId.HasValue)
            {
                dto.RequestTitle = await _db.EngineerRequests
                    .Where(r => r.Id == notification.RequestId.Value)
                    .Select(r => r.RequestTitle)
                    .FirstOrDefaultAsync();
            }

            return dto;
        }

        public async Task<GenericResponse> MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _db.NotificationLogs
                .FirstOrDefaultAsync(n => n.Id == notificationId);

            if (notification is null)
                return GenericResponse.FailureResult("Notification not found.");

            if (notification.IsRead)
                return GenericResponse.SuccessResult("Notification already marked as read.");

            notification.IsRead = true;
                notification.ReadAt = Contracting.Shared.Common.DateTimeHelper.Now;
            await _db.SaveChangesAsync();

            return GenericResponse.SuccessResult("Notification marked as read.");
        }

        public async Task<GenericResponse> MarkAllAsReadAsync(Guid engineerId)
        {
            var unread = await _db.NotificationLogs
                .Where(n => n.EngineerId == engineerId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
                return GenericResponse.SuccessResult("No unread notifications.");

            var now = Contracting.Shared.Common.DateTimeHelper.Now;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }

            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult($"{unread.Count} notifications marked as read.");
        }

        public async Task<int> GetUnreadCountAsync(Guid engineerId)
        {
            return await _db.NotificationLogs
                .Where(n => n.EngineerId == engineerId && !n.IsRead)
                .CountAsync();
        }

        // ---------------------------------------------------------------------
        // Current-user (token-resolved) variants — keyed on UserId so they work
        // for every recipient, including clients/team members who are not engineers.
        // ---------------------------------------------------------------------

        public async Task<PaginatedList<GetNotificationDto>> GetMyNotificationsAsync(NotificationFilterDto filter)
        {
            if (!Guid.TryParse(CurrentUser.UserId, out var userId) || userId == Guid.Empty)
            {
                return new PaginatedList<GetNotificationDto>(
                    new List<GetNotificationDto>(), 0, filter.PageIndex, filter.PageSize);
            }

            var query = _db.NotificationLogs
                .Include(n => n.Engineer)
                .Where(n => n.UserId == userId)
                .AsNoTracking();

            if (filter.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == filter.IsRead.Value);
            }

            query = query.OrderByDescending(n => n.CreatedDate);

            var totalCount = await query.CountAsync();

            if (totalCount == 0)
            {
                return new PaginatedList<GetNotificationDto>(
                    new List<GetNotificationDto>(), 0, filter.PageIndex, filter.PageSize);
            }

            var notifications = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            var requestIds = notifications
                .Where(n => n.RequestId.HasValue)
                .Select(n => n.RequestId!.Value)
                .Distinct()
                .ToList();

            var requestTitles = requestIds.Any()
                ? await _db.EngineerRequests
                    .Where(r => requestIds.Contains(r.Id))
                    .Select(r => new { r.Id, r.RequestTitle })
                    .ToDictionaryAsync(r => r.Id, r => r.RequestTitle)
                : new Dictionary<Guid, string?>();

            var dtos = notifications.Select(n =>
            {
                var dto = _mapper.Map<GetNotificationDto>(n);
                dto.EngineerName = n.Engineer != null
                    ? $"{n.Engineer.nameEn} / {n.Engineer.nameAr}"
                    : null;
                dto.RequestTitle = n.RequestId.HasValue && requestTitles.TryGetValue(n.RequestId.Value, out var title)
                    ? title
                    : null;
                return dto;
            }).ToList();

            return new PaginatedList<GetNotificationDto>(
                dtos, totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<GenericResponse> MarkAllAsReadForCurrentUserAsync()
        {
            if (!Guid.TryParse(CurrentUser.UserId, out var userId) || userId == Guid.Empty)
                return GenericResponse.FailureResult("User not found.");

            var unread = await _db.NotificationLogs
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            if (unread.Count == 0)
                return GenericResponse.SuccessResult("No unread notifications.");

            var now = Contracting.Shared.Common.DateTimeHelper.Now;
            foreach (var n in unread)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }

            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult($"{unread.Count} notifications marked as read.");
        }

        public async Task<int> GetMyUnreadCountAsync()
        {
            if (!Guid.TryParse(CurrentUser.UserId, out var userId) || userId == Guid.Empty)
                return 0;

            return await _db.NotificationLogs
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

    }
}
