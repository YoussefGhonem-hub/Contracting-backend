using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.LaborAttendance.Command.CreateLaborAttendance;
using Contracting.Application.Features.Business.LaborAttendance.Command.DeleteLaborAttendance;
using Contracting.Application.Features.Business.LaborAttendance.Command.TakeActionLaborAttendance;
using Contracting.Application.Features.Business.LaborAttendance.Command.UpdateLaborAttendance;
using Contracting.Application.Features.Business.LaborAttendance.Query.GetAllLaborAttendances;
using Contracting.Application.Features.Business.LaborAttendance.Query.GetLaborAttendanceById;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LaborAttendanceController : APIBaseController
    {
        private readonly IMediator _mediator;
        public LaborAttendanceController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateLaborAttendanceRequestDto dto)
        {
            var result = await _mediator.Send(new CreateLaborAttendanceCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateLaborAttendanceRequestDto dto)
        {
            var result = await _mediator.Send(new UpdateLaborAttendanceCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteLaborAttendanceCommand(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetLaborAttendanceByIdQuery(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] LaborAttendanceFilterDto filter)
        {
            var result = await _mediator.Send(new GetAllLaborAttendancesQuery(filter));
            return Ok(result);
        }

        [HttpPost("{id:guid}/action")]
        public async Task<IActionResult> TakeAction(Guid id, [FromBody] LaborAttendanceActionDto dto)
        {
            var result = await _mediator.Send(new TakeActionLaborAttendanceCommand(id, dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }
    }
}
