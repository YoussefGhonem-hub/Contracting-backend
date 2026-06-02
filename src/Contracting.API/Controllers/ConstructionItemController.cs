using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.ConstructionItem.Command.CreateConstructionItem;
using Contracting.Application.Features.Master.ConstructionItem.Command.DeleteConstructionItem;
using Contracting.Application.Features.Master.ConstructionItem.Command.UpdateConstructionItem;
using Contracting.Application.Features.Master.ConstructionItem.Query.GetAllConstructionItems;
using Contracting.Application.Features.Master.ConstructionItem.Query.GetConstructionItemById;
using Contracting.Application.Features.Master.ConstructionItem.Query.GetConstructionItemDropdown;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ConstructionItemController : APIBaseController
    {
        private readonly IMediator _mediator;

        public ConstructionItemController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Create([FromBody] CreateConstructionItemDto dto)
        {
            var command = new CreateConstructionItemCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                item => Ok(item),
                errors => Problem(errors)
            );
        }

        // Update
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Update([FromBody] UpdateConstructionItemDto dto)
        {
            var command = new UpdateConstructionItemCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                item => Ok(item),
                errors => Problem(errors)
            );
        }

        // Delete
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteConstructionItemCommand(id);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get All (paginated)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BaseFilterDto filter)
        {
            var query = new GetAllConstructionItemsQuery(filter);
            var result = await _mediator.Send(query);

            return result.Match(
                items => Ok(items),
                errors => Problem(errors)
            );
        }

        // Get By Id
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetConstructionItemByIdQuery(id);
            var result = await _mediator.Send(query);

            return result.Match(
                item => Ok(item),
                errors => Problem(errors)
            );
        }

        // Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown()
        {
            var query = new GetConstructionItemDropdownQuery();
            var result = await _mediator.Send(query);

            return result.Match(
                items => Ok(items),
                errors => Problem(errors)
            );
        }
    }
}
