using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetUnreadNotificationCount
{
    public record GetUnreadNotificationCountQuery(Guid EngineerId) : IRequest<ErrorOr<int>>;
}
