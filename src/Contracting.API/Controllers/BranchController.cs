using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.Branch.Command.CreateBranch;
using Contracting.Application.Features.Master.Branch.Command.DeleteBranch;
using Contracting.Application.Features.Master.Branch.Command.UpdateBranch;
using Contracting.Application.Features.Master.Branch.Query.GetAllBranches;
using Contracting.Application.Features.Master.Branch.Query.GetBanchDropDown;
using Contracting.Application.Features.Master.Branch.Query.GetBranchById;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.BranchDto;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BranchController : APIBaseController
    {
        private readonly IMediator _mediator;

        public BranchController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateBranchDto dto)
        {
            var command = new CreateBranchCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                branch => CreatedAtAction(nameof(GetById), new { id = branch.Id }, branch),
                errors => Problem(errors)
            );
        }

        // ---------------- UPDATE ----------------
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateBranchDto dto)
        {
            
            var command = new UpdateBranchCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                result =>Ok(result),
                errors => Problem(errors)
            );
        }

        // ---------------- DELETE ----------------
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteBranchCommand(id);
            var result = await _mediator.Send(command);

            return result.Match(
                result => Ok(result),
                errors => Problem(errors)
            );
        }

        // ---------------- GET BY ID ----------------
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var query = new GetBranchByIdQuery(id);
            ErrorOr<GetBranchDto> result = await _mediator.Send(query);

            return result.Match(
                branch => Ok(branch),
                errors => Problem(errors)
            );
        }

        // ---------------- GET ALL (PAGINATION) ----------------
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] BaseFilterDto filter)
        {
            var query = new GetAllBranchesQuery(filter);
            ErrorOr<PaginatedList<GetBranchDto>> result =
                await _mediator.Send(query);

            return result.Match(
                paged => Ok(paged),
                errors => Problem(errors)
            );
        }

        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropDown()
        {
            var query = new GetBanchDropDownQuery();
            ErrorOr<List<BranchDropDownDto>> result =
                await _mediator.Send(query);

            return result.Match(
                paged => Ok(paged),
                errors => Problem(errors)
            );
        }
    }
}
