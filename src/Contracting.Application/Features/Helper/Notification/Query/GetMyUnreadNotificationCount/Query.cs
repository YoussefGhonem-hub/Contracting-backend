using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetMyUnreadNotificationCount
{
    public record GetMyUnreadNotificationCountQuery() : IRequest<ErrorOr<int>>;
}
