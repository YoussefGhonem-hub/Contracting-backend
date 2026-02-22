using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformance;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformanceByEngineerId;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAgingReport;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetLeadCycleTime;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetOfficeEngineerAnalysis;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetOverdueRisk;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetReworkRate;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetSiteEngineerAnalysis;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetSlaBucketsReport;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetTeamLeadAnalysis;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentage;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestStatusPercentageByEngineerId;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetWeeklyCompletion;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    /// <summary>
    /// Controller for engineer request analytics and performance metrics.
    /// Provides various reports and analysis for monitoring engineer requests workflow.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    [Produces("application/json")]
    public class EngineerRequestAnalysisController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerRequestAnalysisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves analysis data for site engineers.
        /// </summary>
        /// <remarks>
        /// Returns performance metrics and workload statistics for site engineers,
        /// including request counts, completion rates, and average response times.
        /// </remarks>
        /// <returns>Site engineer analysis data</returns>
        /// <response code="200">Returns the site engineer analysis data</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("site-engineer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSiteEngineerAnalysis()
        {
            var query = new GetSiteEngineerAnalysisQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves analysis data for office engineers.
        /// </summary>
        /// <remarks>
        /// Returns performance metrics and workload statistics for office engineers,
        /// including request processing times, approval rates, and pending workload.
        /// </remarks>
        /// <returns>Office engineer analysis data</returns>
        /// <response code="200">Returns the office engineer analysis data</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("office-engineer")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOfficeEngineerAnalysis()
        {
            var query = new GetOfficeEngineerAnalysisQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves analysis data for team leads.
        /// </summary>
        /// <remarks>
        /// Returns team-level performance metrics including team productivity,
        /// request distribution, bottlenecks, and overall team efficiency indicators.
        /// </remarks>
        /// <returns>Team lead analysis data</returns>
        /// <response code="200">Returns the team lead analysis data</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("team-lead")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetTeamLeadAnalysis()
        {
            var query = new GetTeamLeadAnalysisQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves SLA (Service Level Agreement) buckets report.
        /// </summary>
        /// <remarks>
        /// Returns requests categorized by SLA compliance status (e.g., within SLA, 
        /// approaching deadline, breached). Useful for monitoring service level adherence.
        /// </remarks>
        /// <returns>SLA buckets report data</returns>
        /// <response code="200">Returns the SLA buckets report</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("sla-buckets")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetSlaBuckets()
        {
            var query = new GetSlaBucketsReportQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves the aging report for engineer requests.
        /// </summary>
        /// <remarks>
        /// Returns requests grouped by age (days pending), helping identify 
        /// stale requests and potential bottlenecks in the workflow process.
        /// </remarks>
        /// <returns>Aging report data</returns>
        /// <response code="200">Returns the aging report</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("aging-report")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAgingReport()
        {
            var query = new GetAgingReportQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves lead cycle time metrics.
        /// </summary>
        /// <remarks>
        /// Returns the average time taken to complete requests from creation to closure,
        /// broken down by stages. Helps identify process inefficiencies and improvement areas.
        /// </remarks>
        /// <returns>Lead cycle time metrics</returns>
        /// <response code="200">Returns the lead cycle time data</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("lead-cycle-time")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetLeadCycleTime()
        {
            var query = new GetLeadCycleTimeQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves overdue risk assessment report.
        /// </summary>
        /// <remarks>
        /// Returns requests that are at risk of becoming overdue or have already exceeded 
        /// their deadlines. Includes risk scoring and priority recommendations.
        /// </remarks>
        /// <returns>Overdue risk assessment data</returns>
        /// <response code="200">Returns the overdue risk report</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("overdue-risk")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetOverdueRisk()
        {
            var query = new GetOverdueRiskQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves assignee performance metrics.
        /// </summary>
        /// <remarks>
        /// Returns individual performance data for each assignee including completed requests,
        /// average completion time, quality scores, and workload distribution.
        /// </remarks>
        /// <returns>Assignee performance metrics</returns>
        /// <response code="200">Returns the assignee performance data</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("assignee-performance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssigneePerformance()
        {
            var query = new GetAssigneePerformanceQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves assignee performance metrics for a specific engineer.
        /// </summary>
        /// <remarks>
        /// Returns performance data for the specified engineer, including completed requests,
        /// average completion time, and workload distribution.
        /// </remarks>
        /// <param name="engineerId">Engineer ID to filter by</param>
        /// <returns>Assignee performance metrics for the engineer</returns>
        /// <response code="200">Returns the assignee performance data</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("assignee-performance/{engineerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAssigneePerformanceByEngineerId(Guid engineerId)
        {
            var query = new GetAssigneePerformanceByEngineerIdQuery(engineerId);
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Retrieves rework rate analysis.
        /// </summary>
        /// <remarks>
        /// Returns metrics on requests that required rework or were returned for corrections.
        /// Includes rework percentage, common reasons, and trends over time.
        /// </remarks>
        /// <returns>Rework rate analysis data</returns>
        /// <response code="200">Returns the rework rate analysis</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("rework-rate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetReworkRate()
        {
            var query = new GetReworkRateQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Get request status percentage analysis for all engineers
        /// </summary>
        /// <remarks>
        /// Returns finished in time, finished before end time coming, delayed, and completed percentages per engineer.
        /// </remarks>
        /// <response code="200">Returns status percentage analysis for all engineers</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("status-percentage")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRequestStatusPercentage([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
        {
            var query = new GetRequestStatusPercentageQuery(startDate, endDate);
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Get request status percentage analysis for a specific engineer
        /// </summary>
        /// <param name="engineerId">The ID of the engineer</param>
        /// <remarks>
        /// Returns finished in time, finished before end time coming, delayed, and completed percentages for the specified engineer.
        /// </remarks>
        /// <response code="200">Returns status percentage analysis for the engineer</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("status-percentage/{engineerId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetRequestStatusPercentageByEngineerId(Guid engineerId, [FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var query = new GetRequestStatusPercentageByEngineerIdQuery(engineerId, month, year);
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        /// <summary>
        /// Get weekly on-time completion vs delayed tasks for a given month
        /// </summary>
        /// <param name="month">Month number (1-12). Defaults to current month.</param>
        /// <param name="year">Year. Defaults to current year.</param>
        /// <remarks>
        /// Returns weekly breakdown of on-time completions and delayed tasks within the specified month.
        /// </remarks>
        /// <response code="200">Returns weekly completion analysis</response>
        /// <response code="401">Unauthorized - User is not authenticated</response>
        [HttpGet("weekly-completion")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetWeeklyCompletion([FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var query = new GetWeeklyCompletionQuery(month, year);
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }
    }
}
