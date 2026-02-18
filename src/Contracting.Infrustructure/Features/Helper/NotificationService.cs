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
            var exists = await _db.userDeviceTokens
                        .AnyAsync(t => t.UserId == Guid.Parse(CurrentUser.UserId)
                            && t.FcmToken == fcmToken);

            if (!exists)
            {
                _db.userDeviceTokens.Add(new UserDeviceToken
                {
                    UserId = Guid.Parse(CurrentUser.UserId),
                    FcmToken = fcmToken
                });
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
                var engineerId = string.IsNullOrEmpty(notification.EngineerId) ? (Guid?)null : Guid.Parse(notification.EngineerId);
                var departmentId = string.IsNullOrEmpty(notification.DepartmentId) ? (Guid?)null : Guid.Parse(notification.DepartmentId);
                var requestId = string.IsNullOrEmpty(notification.RequestId) ? (Guid?)null : Guid.Parse(notification.RequestId);

                var notificationLog = new NotificationLog
                {
                    UserId = Guid.Parse(CurrentUser.UserId),
                    Token = notification.Token,
                    Title = notification.Title,
                    Body = notification.Body,
                    IsSent = isSent,
                    ErrorMessage = errorMessage,
                    EngineerId = engineerId,
                    DepartmentId = departmentId,
                    RequestId = requestId,
                    SentAt = DateTimeOffset.UtcNow
                };

                _db.NotificationLogs.Add(notificationLog);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log notification");
            }
        }

        public async Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null)
        {
            if (userId == Guid.Empty)
                return;

            // Resolve the EngineerId from the ApplicationUserId
            var engineerId = await _db.Engineers
                .Where(e => e.ApplicationUserId == userId)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefaultAsync();

            var tokens = await _db.userDeviceTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.FcmToken)
                .ToListAsync();

            // enqueue a background job per token
            foreach (var token in tokens)
            {
                var notification = new PushNotificationDto
                {
                    Token = token,
                    Title = title,
                    Body = body,
                    EngineerId = engineerId?.ToString(),
                    RequestId = requestId?.ToString(),
                    DepartmentId = departmentId?.ToString()
                };
                try
                {
                    _backgroundJobClient.Enqueue<NotificationService>(svc => svc.SendAsync(notification));
                    await LogNotificationAsync(notification, true, null);
                }
                catch (Exception ex)
                {
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

            var dtos = notifications.Select(n =>
            {
                var dto = _mapper.Map<GetNotificationDto>(n);
                dto.EngineerName = n.Engineer != null
                    ? $"{n.Engineer.nameEn} / {n.Engineer.nameAr}"
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
            notification.ReadAt = DateTimeOffset.UtcNow;
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

            var now = DateTimeOffset.UtcNow;
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

    }
}
