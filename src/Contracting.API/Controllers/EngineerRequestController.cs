using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerRequest.Command.ConfirmDeliveryDate;
using Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.ReassignEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAllRequestsByDepartment;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestsByFilter;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestActivities;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestById;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestCreatedOrApplyToEngineer;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestsByStatusForEngineer;
using Contracting.Application.Features.Business.PurchaseRequest.Command.CreateGoodsReceipt;
using Contracting.Application.Features.Business.PurchaseRequest.Query.GetGoodsReceipts;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using Contracting.Shared.Dtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EngineerRequestController : APIBaseController
    {
        private readonly IMediator _mediator;

        public EngineerRequestController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Engineer Request
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateEngineerRequestDto dto)
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
        public async Task<IActionResult> Update([FromForm] UpdateEngineerRequestDto dto)
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

        // Filter Engineer Requests
        [HttpGet("filter")]
        public async Task<IActionResult> Filter([FromQuery] EngineerRequestFilterDto filter)
        {
            var query = new GetEngineerRequestsByFilterQuery(filter);
            var result = await _mediator.Send(query);

            return result.Match(
                requests => Ok(requests),
                errors => Problem(errors)
            );
        }
        [HttpGet("appliedOrCreatedReqeust")]
        public async Task<IActionResult> GetAllRequestAppliedOrCreated([FromQuery] EngineerRequestParticipationFilterDto filter)
        {
            var query = new GetRequestCreatedOrApplyToEngineerQuery(filter);
            var result = await _mediator.Send(query);

            return result.Match(
                requests => Ok(requests),
                errors => Problem(errors)
            );
        }

        // Get requests by status for current engineer (assigned to or created by)
        [HttpGet("byStatus")]
        public async Task<IActionResult> GetRequestsByStatus([FromQuery] GetRequestsByStatusFilterDto filter)
        {
            var query = new GetRequestsByStatusForEngineerQuery(filter);
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

        [HttpPost("{requestId:guid}/action")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> TakeAction(
            Guid requestId,
            [FromForm] TakeActionRequestDto dto)
        {

            var command = new TakeActionRequestCommand(requestId, dto);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(new { Message = "Request approved successfully" }),
                errors => Problem(errors)
            );
        }

        [HttpPost("{requestId:guid}/reassign")]
        [Consumes("application/json")]
        public async Task<IActionResult> Reassign(
            Guid requestId,
            [FromBody] ReassignEngineerRequestDto dto)
        {
            var command = new ReassignEngineerRequestCommand(requestId, dto);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get Request Activities (status changes, assignments)
        [HttpGet("{requestId:guid}/activities")]
        public async Task<IActionResult> GetActivities(Guid requestId)
        {
            var query = new GetRequestActivitiesQuery(requestId);
            var result = await _mediator.Send(query);

            return result.Match(
                activities => Ok(activities),
                errors => Problem(errors)
            );
        }

        // Confirm Delivery Date — locks endDate against future changes
        [HttpPatch("{requestId:guid}/confirm-delivery-date")]
        public async Task<IActionResult> ConfirmDeliveryDate(Guid requestId)
        {
            var command = new ConfirmDeliveryDateCommand(requestId);
            var result = await _mediator.Send(command);

            return result.Match(
                _ => NoContent(),
                errors => Problem(errors)
            );
        }

        // Goods receipt — record received quantities
        [HttpPost("{requestId:guid}/receipts")]
        public async Task<IActionResult> CreateGoodsReceipt(
            Guid requestId,
            [FromBody] CreateGoodsReceiptDto dto)
        {
            var result = await _mediator.Send(new CreateGoodsReceiptCommand(requestId, dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        // Goods receipts — list all receipt records for a request (audit trail)
        [HttpGet("{requestId:guid}/receipts")]
        public async Task<IActionResult> GetGoodsReceipts(Guid requestId)
        {
            var result = await _mediator.Send(new GetGoodsReceiptsQuery(requestId));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }
    }
}