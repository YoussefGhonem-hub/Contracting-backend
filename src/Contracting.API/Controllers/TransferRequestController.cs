using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.TransferRequest.Command.CreateTransferRequest;
using Contracting.Application.Features.Business.TransferRequest.Command.DeleteTransferRequest;
using Contracting.Application.Features.Business.TransferRequest.Command.TakeActionTransferRequest;
using Contracting.Application.Features.Business.TransferRequest.Command.UpdateTransferRequest;
using Contracting.Application.Features.Business.TransferRequest.Query.GetAllTransferRequests;
using Contracting.Application.Features.Business.TransferRequest.Query.GetPublicTransferRequestReport;
using Contracting.Application.Features.Business.TransferRequest.Query.GetTransferRequestById;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TransferRequestController : APIBaseController
    {
        private readonly IMediator _mediator;
        public TransferRequestController(IMediator mediator) => _mediator = mediator;

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] CreateTransferRequestDto dto)
        {
            var result = await _mediator.Send(new CreateTransferRequestCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromForm] UpdateTransferRequestDto dto)
        {
            var result = await _mediator.Send(new UpdateTransferRequestCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteTransferRequestCommand(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _mediator.Send(new GetTransferRequestByIdQuery(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        // Public printable report — anonymous access by unguessable GUID.
        // Returns the transfer + creator/receiver signatories with signature URLs.
        [HttpGet("public/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicReport(Guid id)
        {
            var result = await _mediator.Send(new GetPublicTransferRequestReportQuery(id));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] TransferRequestFilterDto filter)
        {
            var result = await _mediator.Send(new GetAllTransferRequestsQuery(filter));
            return Ok(result);
        }

        [HttpPost("{id:guid}/action")]
        public async Task<IActionResult> TakeAction(Guid id, [FromBody] TransferRequestActionDto dto)
        {
            var result = await _mediator.Send(new TakeActionTransferRequestCommand(id, dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }
    }
}
