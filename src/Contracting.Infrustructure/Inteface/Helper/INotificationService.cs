using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos.HelperDtos;

namespace Contracting.Infrustructure.Inteface.Helper
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null, NotificationKey? key = null);
        Task SendAsync(PushNotificationDto notification);
        Task<bool> GenerateToken(string token);
        Task<bool> RemoveToken(string token);
        Task<PaginatedList<GetNotificationDto>> GetNotificationsByEngineerAsync(Guid engineerId, NotificationFilterDto filter);
        Task<GetNotificationDto> GetNotificationByIdAsync(Guid notificationId);
        Task<GenericResponse> MarkAsReadAsync(Guid notificationId);
        Task<GenericResponse> MarkAllAsReadAsync(Guid engineerId);
        Task<int> GetUnreadCountAsync(Guid engineerId);
    }
}
