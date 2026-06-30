using Contracting.Infrustructure.Inteface.Helper;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetMyUnreadNotificationCount
{
    public class GetMyUnreadNotificationCountQueryHandler : IRequestHandler<GetMyUnreadNotificationCountQuery, ErrorOr<int>>
    {
        private readonly INotificationService _service;

        public GetMyUnreadNotificationCountQueryHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<int>> Handle(GetMyUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _service.GetMyUnreadCountAsync();
            return count;
        }
    }
}
