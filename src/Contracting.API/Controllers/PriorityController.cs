using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.Priority.Command.CreatePriority;
using Contracting.Application.Features.Master.Priority.Command.DeletePriority;
using Contracting.Application.Features.Master.Priority.Command.UpdatePriority;
using Contracting.Application.Features.Master.Priority.Query.GetAllPriority;
using Contracting.Application.Features.Master.Priority.Query.GetPriorityDropdown;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PriorityController : APIBaseController
    {
        private readonly IMediator _mediator;

        public PriorityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Priority
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePriorityDto dto)
        {
            var command = new CreatePriorityCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                priority => Ok(priority),
                errors => Problem(errors)
            );
        }

        // Update Priority
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdatePriorityDto dto)
        {
            var command = new UpdatePriorityCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                priority => Ok(priority),
                errors => Problem(errors)
            );
        }

        // Delete Priority
        [HttpDelete("{priorityId:guid}")]
        public async Task<IActionResult> Delete(Guid priorityId)
        {
            var command = new DeletePriorityCommand(priorityId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get All Priorities with Pagination
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BaseFilterDto filter)
        {
            var query = new GetAllPriorityQuery(filter);
            var result = await _mediator.Send(query);

            return result.Match(
                priorities => Ok(priorities),
                errors => Problem(errors)
            );
        }

        // Get Priority Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var query = new GetPriorityDropdownQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                priorities => Ok(priorities),
                errors => Problem(errors)
            );
        }
    }
}