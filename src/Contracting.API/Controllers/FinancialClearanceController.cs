using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.FinancialClearance.Command.CreateFinancialClearance;
using Contracting.Application.Features.Business.FinancialClearance.Command.DeleteFinancialClearance;
using Contracting.Application.Features.Business.FinancialClearance.Command.TakeActionFinancialClearance;
using Contracting.Application.Features.Business.FinancialClearance.Command.UpdateFinancialClearance;
using Contracting.Application.Features.Business.FinancialClearance.Query.GetAllFinancialClearances;
using Contracting.Application.Features.Business.FinancialClearance.Query.GetFinancialClearanceById;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FinancialClearanceController : APIBaseController
    {
        private readonly IMediator _mediator;
        public FinancialClearanceController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateFinancialClearanceDto dto)
        {
            var result = await _mediator.Send(new CreateFinancialClearanceCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateFinancialClearanceDto dto)
        {
            var result = await _mediator.Send(new UpdateFinancialClearanceCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteFinancialClearanceCommand(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetFinancialClearanceByIdQuery(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] FinancialClearanceFilterDto filter)
        {
            var result = await _mediator.Send(new GetAllFinancialClearancesQuery(filter));
            return Ok(result);
        }

        [HttpPost("{id:guid}/action")]
        public async Task<IActionResult> TakeAction(Guid id, [FromBody] FinancialClearanceActionDto dto)
        {
            var result = await _mediator.Send(new TakeActionFinancialClearanceCommand(id, dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }
    }
}
