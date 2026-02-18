using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Helper.Notification.Command.MarkAllNotificationsAsRead;
using Contracting.Application.Features.Helper.Notification.Command.MarkNotificationAsRead;
using Contracting.Application.Features.Helper.Notification.Query.GetNotificationById;
using Contracting.Application.Features.Helper.Notification.Query.GetNotificationsByEngineer;
using Contracting.Application.Features.Helper.Notification.Query.GetUnreadNotificationCount;
using Contracting.Shared.Dtos.HelperDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationController : APIBaseController
    {
        private readonly IMediator _mediator;

        public NotificationController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Get notifications for a specific engineer (paginated, optionally filter by IsRead)
        [HttpGet("engineer/{engineerId:guid}")]
        public async Task<IActionResult> GetByEngineer(Guid engineerId, [FromQuery] NotificationFilterDto filter)
        {
            var query = new GetNotificationsByEngineerQuery(engineerId, filter);
            var result = await _mediator.Send(query);

            return result.Match(
                notifications => Ok(notifications),
                errors => Problem(errors)
            );
        }

        // Get a single notification by Id
        [HttpGet("{notificationId:guid}")]
        public async Task<IActionResult> GetById(Guid notificationId)
        {
            var query = new GetNotificationByIdQuery(notificationId);
            var result = await _mediator.Send(query);

            return result.Match(
                notification => Ok(notification),
                errors => Problem(errors)
            );
        }

        // Mark a single notification as read (when the notification is opened)
        [HttpPut("{notificationId:guid}/read")]
        public async Task<IActionResult> MarkAsRead(Guid notificationId)
        {
            var command = new MarkNotificationAsReadCommand(notificationId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Mark all notifications as read for an engineer
        [HttpPut("engineer/{engineerId:guid}/read-all")]
        public async Task<IActionResult> MarkAllAsRead(Guid engineerId)
        {
            var command = new MarkAllNotificationsAsReadCommand(engineerId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get unread notification count for an engineer
        [HttpGet("engineer/{engineerId:guid}/unread-count")]
        public async Task<IActionResult> GetUnreadCount(Guid engineerId)
        {
            var query = new GetUnreadNotificationCountQuery(engineerId);
            var result = await _mediator.Send(query);

            return result.Match(
                count => Ok(new { count }),
                errors => Problem(errors)
            );
        }
    }
}
