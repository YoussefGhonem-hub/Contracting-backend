using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAllRequestsByDepartment;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestById;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EngineerRequestController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerRequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Engineer Request
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateEngineerRequestDto dto)
        {
            var command = new CreateEngineerRequestCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                request => Ok(request),
                errors => Problem(errors)
            );
        }

        // Update Engineer Request
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateEngineerRequestDto dto)
        {
            var command = new UpdateEngineerRequestCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                request => Ok(request),
                errors => Problem(errors)
            );
        }

        // Delete Engineer Request
        [HttpDelete("{requestId:guid}")]
        public async Task<IActionResult> Delete(Guid requestId)
        {
            var command = new DeleteEngineerRequestCommand(requestId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get All Requests by Department (for managers)
        [HttpGet("department/{departmentId:guid}")]
        public async Task<IActionResult> GetAllByDepartment(Guid departmentId, [FromQuery] BaseFilterDto filter)
        {
            var query = new GetAllRequestsByDepartmentQuery(departmentId, filter);
            var result = await _mediator.Send(query);

            return result.Match(
                requests => Ok(requests),
                errors => Problem(errors)
            );
        }

        // Get Request by ID
        [HttpGet("{requestId:guid}")]
        public async Task<IActionResult> GetById(Guid requestId)
        {
            var query = new GetRequestByIdQuery(requestId);
            var result = await _mediator.Send(query);

            return result.Match(
                request => Ok(request),
                errors => Problem(errors)
            );
        }

        // Take Action on Request (Approve/Reject)
        [HttpPost("{requestId:guid}/action")]
        public async Task<IActionResult> TakeAction(
            Guid requestId, 
            [FromQuery] Guid engineerId, 
            [FromQuery] bool isApproved, 
            [FromBody] string? actionNote = null)
        {
            var command = new TakeActionOnRequestCommand(requestId, engineerId, isApproved, actionNote);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(new { Message = isApproved ? "Request approved successfully" : "Request rejected successfully" }),
                errors => Problem(errors)
            );
        }
    }
}