using Contracting.Shared.HelperDtos;

namespace Contracting.Infrustructure.Inteface.Helper
{
    public interface INotificationService
    {
        Task SendAsync(PushNotificationDto notification);
        Task<bool> GenerateToken(string token);
        Task<bool> RemoveToken(string token);

    }
}
