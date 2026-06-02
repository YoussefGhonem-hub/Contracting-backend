using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.SiteReport.Query.GetClientSiteReportById;
using Contracting.Application.Features.Client.SiteReport.Query.GetClientSiteReports;
using Contracting.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/client")]
[ApiController]
[Authorize(Roles = RoleNames.Client)]
public class ClientSiteReportController : APIBaseController
{
    private readonly IMediator _mediator;

    public ClientSiteReportController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all site reports for a specific project assigned to the logged-in client.
    /// Used by the "Site Reports" mobile screen.
    /// </summary>
    [HttpGet("projects/{projectId:guid}/site-reports")]
    public async Task<IActionResult> GetSiteReports(Guid projectId)
    {
        var query = new GetClientSiteReportsQuery(projectId);
        var result = await _mediator.Send(query);

        return result.Match(
            reports => Ok(reports),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Get the full details of a single site report by its ID.
    /// </summary>
    [HttpGet("site-reports/{reportId:guid}")]
    public async Task<IActionResult> GetSiteReportById(Guid reportId)
    {
        var query = new GetClientSiteReportByIdQuery(reportId);
        var result = await _mediator.Send(query);

        return result.Match(
            report => Ok(report),
            errors => Problem(errors)
        );
    }
}
