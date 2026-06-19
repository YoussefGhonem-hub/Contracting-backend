using Contracting.API.Controllers.Shared;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Dtos.AnalyticsDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/performance")]
[ApiController]
[Authorize]
public class PerformanceAnalyticsController : APIBaseController
{
    private readonly IPerformanceAnalyticsService _service;

    public PerformanceAnalyticsController(IPerformanceAnalyticsService service)
    {
        _service = service;
    }

    /// <summary>
    /// GET /api/performance/site-analytics
    ///
    /// Query params:
    ///   departmentId  — filter by dept (super/admin/team-lead)
    ///   engineerId    — scope to one engineer (super/admin/team-lead in same dept)
    ///   fromDate      — start of period (yyyy-MM-dd), default: last 30 days
    ///   toDate        — end of period   (yyyy-MM-dd), default: today
    ///
    /// Visibility:
    ///   SuperAdmin / Admin      → all departments (can filter)
    ///   Teamlead-engineer       → own department only (can filter by engineer)
    ///   Site/Office engineer    → self only
    /// </summary>
    [HttpGet("site-analytics")]
    public async Task<IActionResult> GetSiteAnalytics(
        [FromQuery] PerformanceAnalyticsFilterDto filter,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetSiteAnalyticsAsync(filter, cancellationToken);
        return result.Match(dto => Ok(dto), errors => Problem(errors));
    }
}
