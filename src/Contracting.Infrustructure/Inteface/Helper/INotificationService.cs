using Contracting.Shared.HelperDtos;

namespace Contracting.Infrustructure.Inteface.Helper
{
    public interface INotificationService
    {
        Task SendNotificationToUserAsync(Guid userId, string title, string body, Guid? requestId = null, Guid? departmentId = null);
        Task SendAsync(PushNotificationDto notification);
        Task<bool> GenerateToken(string token);
        Task<bool> RemoveToken(string token);

    }
}
