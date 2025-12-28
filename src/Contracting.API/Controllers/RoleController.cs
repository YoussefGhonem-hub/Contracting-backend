using Contracting.Application.Features.Role.Command.CreateRoleCommand;
using Contracting.Application.Features.Role.Command.DeleteRoleCommand;
using Contracting.Application.Features.Role.Command.UpdateRoleCommand;
using Contracting.Shared.MasterDtos.RoleDto;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RoleController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRoleDto dto)
        {
            var command = new CreateRoleCommand(dto);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
        {
            var command = new UpdateRoleCommand(dto);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteRoleCommand(id);
            await _mediator.Send(command);
            return Ok();
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var query = new GetRolesDropdownQuery();
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
