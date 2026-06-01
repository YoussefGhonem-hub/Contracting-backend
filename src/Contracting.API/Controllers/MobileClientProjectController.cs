using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.ClientProject.Query.GetClientProjects;
using Contracting.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/mobile/client")]
[ApiController]
[Authorize(Roles = RoleNames.Client)]
public class MobileClientProjectController : APIBaseController
{
    private readonly IMediator _mediator;

    public MobileClientProjectController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all projects assigned to the currently logged-in client.
    /// Used by the "Select Project" mobile screen.
    /// </summary>
    [HttpGet("projects")]
    public async Task<IActionResult> GetMyProjects()
    {
        var query = new GetClientProjectsQuery();
        var result = await _mediator.Send(query);

        return result.Match(
            projects => Ok(projects),
            errors => Problem(errors)
        );
    }
}
