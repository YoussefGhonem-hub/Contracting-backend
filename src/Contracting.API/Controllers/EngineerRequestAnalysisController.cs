using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetOfficeEngineerAnalysis;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetSiteEngineerAnalysis;
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
    }
}
