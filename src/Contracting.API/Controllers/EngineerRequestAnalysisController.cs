using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAssigneePerformance;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAgingReport;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetLeadCycleTime;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetOfficeEngineerAnalysis;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetOverdueRisk;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetReworkRate;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetSiteEngineerAnalysis;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetSlaBucketsReport;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetTeamLeadAnalysis;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EngineerRequestAnalysisController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerRequestAnalysisController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("site-engineer")]
        public async Task<IActionResult> GetSiteEngineerAnalysis()
        {
            var query = new GetSiteEngineerAnalysisQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("office-engineer")]
        public async Task<IActionResult> GetOfficeEngineerAnalysis()
        {
            var query = new GetOfficeEngineerAnalysisQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("team-lead")]
        public async Task<IActionResult> GetTeamLeadAnalysis()
        {
            var query = new GetTeamLeadAnalysisQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("sla-buckets")]
        public async Task<IActionResult> GetSlaBuckets()
        {
            var query = new GetSlaBucketsReportQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("aging-report")]
        public async Task<IActionResult> GetAgingReport()
        {
            var query = new GetAgingReportQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("lead-cycle-time")]
        public async Task<IActionResult> GetLeadCycleTime()
        {
            var query = new GetLeadCycleTimeQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("overdue-risk")]
        public async Task<IActionResult> GetOverdueRisk()
        {
            var query = new GetOverdueRiskQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("assignee-performance")]
        public async Task<IActionResult> GetAssigneePerformance()
        {
            var query = new GetAssigneePerformanceQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

        [HttpGet("rework-rate")]
        public async Task<IActionResult> GetReworkRate()
        {
            var query = new GetReworkRateQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }
    }
}
