using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Command.MarkNotificationAsRead
{
    public class MarkNotificationAsReadCommandHandler : IRequestHandler<MarkNotificationAsReadCommand, ErrorOr<GenericResponse>>
    {
        private readonly INotificationService _service;

        public MarkNotificationAsReadCommandHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(MarkNotificationAsReadCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.MarkAsReadAsync(request.NotificationId);
            return result.Success
                ? result
                : Error.NotFound(result.Message ?? "Notification not found.");
        }
    }
}
