using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Command.MarkAllMyNotificationsAsRead
{
    public class MarkAllMyNotificationsAsReadCommandHandler : IRequestHandler<MarkAllMyNotificationsAsReadCommand, ErrorOr<GenericResponse>>
    {
        private readonly INotificationService _service;

        public MarkAllMyNotificationsAsReadCommandHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(MarkAllMyNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.MarkAllAsReadForCurrentUserAsync();
            return result;
        }
    }
}
