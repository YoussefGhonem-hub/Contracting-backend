using Contracting.Application.Features.Master.Engineer.Command.CreateEngineer;
using Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer;
using Contracting.Application.Features.Master.Engineer.Command.DeleteEngineer;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerList;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerById;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerDropdown;
using Contracting.Shared.MasterDtos.EngineerDto;
using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Contracting.API.Controllers.Shared;
using Contracting.Shared.Dtos;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineerController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Engineer
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEngineerDto dto)
        {
            var command = new CreateEngineerCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                engineer => Ok(engineer),
                errors => Problem(errors)
            );
        }

        // Update Engineer
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEngineerDto dto)
        {
            var command = new UpdateEngineerCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                engineer => Ok(engineer),
                errors => Problem(errors)
            );
        }

        // Delete Engineer
        [HttpDelete("{engineerId:guid}")]
        public async Task<IActionResult> Delete(Guid engineerId)
        {
            var command = new DeleteEngineerCommand(engineerId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get Engineer List (with filter)
        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] string departmentId, [FromQuery] BaseFilterDto filter)
        {
            var query = new GetEngineerListQuery(departmentId, filter);
            var result = await _mediator.Send(query);

            return result.Match(
                engineers => Ok(engineers),
                errors => Problem(errors)
            );
        }

        // Get Engineer By Id
        [HttpGet("{engineerId:guid}")]
        public async Task<IActionResult> GetById(Guid engineerId)
        {
            var query = new GetEngineerByIdQuery(engineerId);
            var result = await _mediator.Send(query);

            return result.Match(
                engineer => Ok(engineer),
                errors => Problem(errors)
            );
        }

        // Get Engineer Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var query = new GetEngineerDropdownQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                engineers => Ok(engineers),
                errors => Problem(errors)
            );
        }
    }
}
