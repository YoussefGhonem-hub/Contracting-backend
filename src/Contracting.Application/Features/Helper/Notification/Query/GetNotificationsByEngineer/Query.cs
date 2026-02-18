using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos.HelperDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Helper.Notification.Query.GetNotificationsByEngineer
{
    public record GetNotificationsByEngineerQuery(Guid EngineerId, NotificationFilterDto Filter) : IRequest<ErrorOr<PaginatedList<GetNotificationDto>>>;
}
