using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetNotificationById
{
    public record GetNotificationByIdQuery(Guid NotificationId) : IRequest<ErrorOr<GetNotificationDto>>;
}
