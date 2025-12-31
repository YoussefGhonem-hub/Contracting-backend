using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.Status.Command.CreateStatus;
using Contracting.Application.Features.Master.Status.Command.DeleteStatus;
using Contracting.Application.Features.Master.Status.Command.UpdateStatus;
using Contracting.Application.Features.Master.Status.Query.GetStatusDropdown;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatusController : APIBaseController
    {
        private readonly IMediator _mediator;

        public StatusController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Status
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateStatusDto dto)
        {
            var command = new CreateStatusCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                Status => Ok(Status),
                errors => Problem(errors)
            );
        }

        // Update Status
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateStatusDto dto)
        {
            var command = new UpdateStatusCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                Status => Ok(Status),
                errors => Problem(errors)
            );
        }

        // Delete Status
        [HttpDelete("{StatusId:guid}")]
        public async Task<IActionResult> Delete(Guid StatusId)
        {
            var command = new DeleteStatusCommand(StatusId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get Status Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var query = new GetStatusDropdownQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                priorities => Ok(priorities),
                errors => Problem(errors)
            );
        }
    }
}