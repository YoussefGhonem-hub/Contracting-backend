using Contracting.Infrustructure.Inteface.Helper;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetUnreadNotificationCount
{
    public class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, ErrorOr<int>>
    {
        private readonly INotificationService _service;

        public GetUnreadNotificationCountQueryHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<int>> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        {
            var count = await _service.GetUnreadCountAsync(request.EngineerId);
            return count;
        }
    }
}
