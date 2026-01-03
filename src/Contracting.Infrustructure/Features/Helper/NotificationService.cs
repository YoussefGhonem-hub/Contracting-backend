using Contracting.Domain.Entities.helper;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.HelperDtos;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Builder.Extensions;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data.Entity;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Contracting.Infrustructure.Features.Helper
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;
        private readonly ApplicationDbContext _db;
        private readonly IBackgroundJobClient _backgroundJobClient;

        public NotificationService(ILogger<NotificationService> logger, ApplicationDbContext db, IOptions<FirebaseSettings> firebaseOptions, IBackgroundJobClient backgroundJobClient)
        {
            _logger = logger;

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
                _db.SaveChanges();
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

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send Firebase notification");
            }
        }

        public Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null)
        {
            if (userId == Guid.Empty)
                return Task.CompletedTask;

            var tokens = _db.userDeviceTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.FcmToken)
                .ToListAsync();

            // enqueue a background job per token
            tokens.ContinueWith(tks =>
            {
                foreach (var token in tks.Result)
                {
                    var notification = new PushNotificationDto
                    {
                        Token = token,
                        Title = title,
                        Body = body,
                        RequestId = requestId?.ToString(),
                        DepartmentId = departmentId?.ToString()
                    };
                    try
                    {
                        _backgroundJobClient.Enqueue<NotificationService>(svc => svc.SendAsync(notification));
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to enqueue notification job");
                    }
                }
            });

            return Task.CompletedTask;
        }

    }
}
