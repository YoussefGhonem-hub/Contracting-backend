using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetMyNotifications
{
    public class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, ErrorOr<PaginatedList<GetNotificationDto>>>
    {
        private readonly INotificationService _service;

        public GetMyNotificationsQueryHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetNotificationDto>>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetMyNotificationsAsync(request.Filter);
            return result;
        }
    }
}
