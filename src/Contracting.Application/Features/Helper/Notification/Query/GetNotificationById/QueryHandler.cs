using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetNotificationById
{
    public class GetNotificationByIdQueryHandler : IRequestHandler<GetNotificationByIdQuery, ErrorOr<GetNotificationDto>>
    {
        private readonly INotificationService _service;

        public GetNotificationByIdQueryHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetNotificationDto>> Handle(GetNotificationByIdQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetNotificationByIdAsync(request.NotificationId);
            return result is null
                ? Error.NotFound("Notification not found.")
                : result;
        }
    }
}
