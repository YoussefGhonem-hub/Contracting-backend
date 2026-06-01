using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.Schedule.Query.GetClientSchedule;
using Contracting.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/mobile/client")]
[ApiController]
[Authorize(Roles = RoleNames.Client)]
public class MobileClientScheduleController : APIBaseController
{
    private readonly IMediator _mediator;

    public MobileClientScheduleController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get the Planning &amp; Schedule page for a project.
    /// Returns the summary card (dates, duration, progress) plus two tab contents:
    /// Monthly Reports (downloadable report files) and Project Timeline (schedule/Gantt documents).
    /// Used by the "Planning &amp; Schedule" mobile screen.
    /// </summary>
    [HttpGet("projects/{projectId:guid}/schedule")]
    public async Task<IActionResult> GetSchedule(Guid projectId)
    {
        var query = new GetClientScheduleQuery(projectId);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }
}
