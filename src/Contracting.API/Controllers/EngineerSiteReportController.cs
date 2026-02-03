using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerSiteReport.Command.CreateEngineerSiteReport;
using Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportsByEngineerId;
using Contracting.Application.Features.Business.EngineerSiteReport.Query.GetEngineerSiteReportById;
using Contracting.Application.Features.Business.EngineerSiteReport.Query.GetMyEngineerSiteReports;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EngineerSiteReportController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerSiteReportController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateEngineerSiteReportDto dto)
        {
            var command = new CreateEngineerSiteReportCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                report => Ok(report),
                errors => Problem(errors)
            );
        }

        [HttpGet("{reportId:guid}")]
        public async Task<IActionResult> GetById(Guid reportId)
        {
            var query = new GetEngineerSiteReportByIdQuery(reportId);
            var result = await _mediator.Send(query);

            return result.Match(
                report => Ok(report),
                errors => Problem(errors)
            );
        }

        [HttpGet("my")]
        public async Task<IActionResult> GetMyReports([FromQuery] EngineerSiteReportFilterDto filter)
        {
            var query = new GetMyEngineerSiteReportsQuery(filter);
            var result = await _mediator.Send(query);

            return result.Match(
                reports => Ok(reports),
                errors => Problem(errors)
            );
        }

        [HttpGet("engineer/{engineerId:guid}")]
        public async Task<IActionResult> GetByEngineerId(Guid engineerId, [FromQuery] EngineerSiteReportFilterDto filter)
        {
            var query = new GetEngineerSiteReportsByEngineerIdQuery(engineerId, filter);
            var result = await _mediator.Send(query);

            return result.Match(
                reports => Ok(reports),
                errors => Problem(errors)
            );
        }

        [AllowAnonymous]
        [HttpGet("engineerReport/{engineerId:guid}")]
        public async Task<IActionResult> GetReprotByEngineerId(Guid engineerId, [FromQuery] EngineerSiteReportFilterDto filter)
        {
            var query = new GetEngineerSiteReportsByEngineerIdQuery(engineerId, filter);
            var result = await _mediator.Send(query);

            return result.Match(
                reports => Ok(reports),
                errors => Problem(errors)
            );
        }
    }
}
