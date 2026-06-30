using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Command.MarkAllMyNotificationsAsRead
{
    public record MarkAllMyNotificationsAsReadCommand() : IRequest<ErrorOr<GenericResponse>>;
}
