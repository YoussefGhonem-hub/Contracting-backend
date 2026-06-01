using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.VariationOrder.Command.ApproveVariationOrder;
using Contracting.Application.Features.Client.VariationOrder.Command.RejectVariationOrder;
using Contracting.Application.Features.Client.VariationOrder.Query.GetClientVariationOrderById;
using Contracting.Application.Features.Client.VariationOrder.Query.GetClientVariationOrders;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/mobile/client")]
[ApiController]
[Authorize(Roles = RoleNames.Client)]
public class MobileClientVariationOrderController : APIBaseController
{
    private readonly IMediator _mediator;

    public MobileClientVariationOrderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get variation orders for a project with aggregate summary (total approved, total pending).
    /// Used by the "Variation Orders" list screen.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="status">Optional filter: All (omit) | Approved | Pending | Rejected</param>
    [HttpGet("projects/{projectId:guid}/variation-orders")]
    public async Task<IActionResult> GetVariationOrders(Guid projectId, [FromQuery] string? status = null)
    {
        var query = new GetClientVariationOrdersQuery(projectId, status);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get full details of a single variation order including attachments.
    /// Used by the "Variation Orders" detail screen.
    /// </summary>
    [HttpGet("variation-orders/{voId:guid}")]
    public async Task<IActionResult> GetById(Guid voId)
    {
        var query = new GetClientVariationOrderByIdQuery(voId);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Client approves a pending variation order.
    /// Only works if the VO status is Pending.
    /// </summary>
    [HttpPost("variation-orders/{voId:guid}/approve")]
    public async Task<IActionResult> Approve(Guid voId)
    {
        var command = new ApproveVariationOrderCommand(voId);
        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Client rejects a pending variation order.
    /// Only works if the VO status is Pending.
    /// </summary>
    [HttpPost("variation-orders/{voId:guid}/reject")]
    public async Task<IActionResult> Reject(Guid voId, [FromBody] ClientVOActionDto body)
    {
        var command = new RejectVariationOrderCommand(voId, body.RejectionReason);
        var result = await _mediator.Send(command);
        return result.Match(Ok, Problem);
    }
}
