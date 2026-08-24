using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.CreateRequestTypeDefaultDepartment;
using Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.DeleteRequestTypeDefaultDepartment;
using Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.UpdateRequestTypeDefaultDepartment;
using Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Query.GetAllRequestTypeDefaultDepartments;
using Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Query.GetDefaultDepartment;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    /// <summary>
    /// Configures which department a given request type (e.g. FinancialClearance, LaborAttendance)
    /// should default to, per branch. The create-request page calls <see cref="GetDefault"/> for its
    /// request type; if none is configured, the engineer picks the department manually.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RequestTypeDefaultDepartmentController : APIBaseController
    {
        private readonly IMediator _mediator;

        public RequestTypeDefaultDepartmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create default-department config for a branch/request-type
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Create([FromBody] CreateRequestTypeDefaultDepartmentDto dto)
        {
            var command = new CreateRequestTypeDefaultDepartmentCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                config => Ok(config),
                errors => Problem(errors)
            );
        }

        // Update the department for an existing config
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Update([FromBody] UpdateRequestTypeDefaultDepartmentDto dto)
        {
            var command = new UpdateRequestTypeDefaultDepartmentCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                config => Ok(config),
                errors => Problem(errors)
            );
        }

        // Delete a config
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteRequestTypeDefaultDepartmentCommand(id);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // All configured defaults for a branch (admin config page)
        [HttpGet("{branchId:guid}")]
        public async Task<IActionResult> GetAll(Guid branchId)
        {
            var query = new GetAllRequestTypeDefaultDepartmentsQuery(branchId);
            var result = await _mediator.Send(query);

            return result.Match(
                configs => Ok(configs),
                errors => Problem(errors)
            );
        }

        // Lookup called by the create-request page: returns the configured default department for
        // this branch/request-type, or HasDefault=false if none is configured (client falls back to
        // manual department selection by the engineer).
        [HttpGet("default/{branchId:guid}/{requestType}")]
        public async Task<IActionResult> GetDefault(Guid branchId, string requestType)
        {
            var query = new GetDefaultDepartmentQuery(branchId, requestType);
            var result = await _mediator.Send(query);

            return result.Match(
                config => Ok(config),
                errors => Problem(errors)
            );
        }
    }
}
