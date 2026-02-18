using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetNotificationsByEngineer
{
    public class GetNotificationsByEngineerQueryHandler : IRequestHandler<GetNotificationsByEngineerQuery, ErrorOr<PaginatedList<GetNotificationDto>>>
    {
        private readonly INotificationService _service;

        public GetNotificationsByEngineerQueryHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<GetNotificationDto>>> Handle(GetNotificationsByEngineerQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetNotificationsByEngineerAsync(request.EngineerId, request.Filter);
            return result;
        }
    }
}
