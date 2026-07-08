using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos.HelperDtos;

namespace Contracting.Infrustructure.Inteface.Helper
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null, Guid? chatGroupId = null, string? type = null);
        /// <summary>Fan-out variant — always delivers even when userId == current acting user. Use for department-wide notifications where the actor is also a recipient (e.g. team lead approves and should still see the dept notification).</summary>
        Task SendFanOutNotificationAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null, Guid? chatGroupId = null, string? type = null);
        Task SendAsync(PushNotificationDto notification);
        Task<bool> GenerateToken(string token);
        Task<bool> RemoveToken(string token);
        Task<PaginatedList<GetNotificationDto>> GetNotificationsByEngineerAsync(Guid engineerId, NotificationFilterDto filter);
        Task<GetNotificationDto> GetNotificationByIdAsync(Guid notificationId);
        Task<GenericResponse> MarkAsReadAsync(Guid notificationId);
        Task<GenericResponse> MarkAllAsReadAsync(Guid engineerId);
        Task<int> GetUnreadCountAsync(Guid engineerId);

        // Current-user (token-resolved) variants — work for every user type, including clients and
        // team members who have no Engineer record (e.g. chat notifications).
        Task<PaginatedList<GetNotificationDto>> GetMyNotificationsAsync(NotificationFilterDto filter);
        Task<GenericResponse> MarkAllAsReadForCurrentUserAsync();
        Task<int> GetMyUnreadCountAsync();
    }
}
