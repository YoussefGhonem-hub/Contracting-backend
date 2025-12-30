using Contracting.API.Controllers.Shared;
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
    public class RoleController : APIBaseController
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
            var result = await _mediator.Send(command);

            return result.Match(
                response => Ok(response),
                errors => Problem(errors)
            );
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto dto)
        {
            var command = new UpdateRoleCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                response => Ok(response),
                errors => Problem(errors)
            );
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteRoleCommand(id);
            var result = await _mediator.Send(command);

            return result.Match(
                response => Ok(response),
                errors => Problem(errors)
            );
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
