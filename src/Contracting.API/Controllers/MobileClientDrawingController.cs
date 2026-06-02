using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.Drawing.Query.GetClientDrawings;
using Contracting.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/mobile/client")]
[ApiController]
[Authorize(Roles = RoleNames.Client)]
public class MobileClientDrawingController : APIBaseController
{
    private readonly IMediator _mediator;

    public MobileClientDrawingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all drawings and renders for a project.
    /// Optionally filter by type: TwoD or ThreeD.
    /// </summary>
    [HttpGet("projects/{projectId:guid}/drawings")]
    public async Task<IActionResult> GetDrawings(
        Guid projectId,
        [FromQuery] string? type = null)
    {
        var query = new GetClientDrawingsQuery(projectId, type);
        var result = await _mediator.Send(query);
        return result.Match(d => Ok(d), errors => Problem(errors));
    }
}
