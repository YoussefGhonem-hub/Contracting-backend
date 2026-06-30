using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetMyNotifications
{
    public record GetMyNotificationsQuery(NotificationFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetNotificationDto>>>;
}
