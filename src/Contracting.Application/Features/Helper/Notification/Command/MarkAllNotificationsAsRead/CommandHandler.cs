using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Command.MarkAllNotificationsAsRead
{
    public class MarkAllNotificationsAsReadCommandHandler : IRequestHandler<MarkAllNotificationsAsReadCommand, ErrorOr<GenericResponse>>
    {
        private readonly INotificationService _service;

        public MarkAllNotificationsAsReadCommandHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(MarkAllNotificationsAsReadCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.MarkAllAsReadAsync(request.EngineerId);
            return result;
        }
    }
}
