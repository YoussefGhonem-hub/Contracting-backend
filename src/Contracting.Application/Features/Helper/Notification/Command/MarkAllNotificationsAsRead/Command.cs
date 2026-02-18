using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Command.MarkAllNotificationsAsRead
{
    public record MarkAllNotificationsAsReadCommand(Guid EngineerId) : IRequest<ErrorOr<GenericResponse>>;
}
