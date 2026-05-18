using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.Department.Command.CreateDepartment;
using Contracting.Application.Features.Master.Department.Command.RemoveDepartment;
using Contracting.Application.Features.Master.Department.Command.UpdateDepartment;
using Contracting.Application.Features.Master.Department.Query.GetDepartmentsByBranchId;
using Contracting.Application.Features.Master.Department.Query.GetDropDownDepartment;
using Contracting.Application.Features.Master.Department.Query.GetDepartmentSpecialFields;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DepartmentController : APIBaseController
    {
        private readonly IMediator _mediator;

        public DepartmentController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Department
        [HttpPost("{branchId:guid}")]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Create(Guid branchId, [FromBody] CreateDepartmentDto dto)
        {
            var command = new CreateDepartmentCommand(branchId, dto);
            var result = await _mediator.Send(command);

            return result.Match(
                department => Ok(department),
                errors => Problem(errors)
            );
        }

        // Update Department
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Update([FromBody] UpdateDepartmentDto dto)
        {
            var command = new UpdateDepartmentCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                department => Ok(department),
                errors => Problem(errors)
            );
        }

        // Get Departments by BranchId (with pagination)
        [HttpGet("{branchId:guid}")]
        public async Task<IActionResult> GetAll(Guid branchId, [FromQuery] BaseFilterDto filter)
        {
            var query = new GetDepartmentsByBranchIdQuery(branchId, filter);
            var result = await _mediator.Send(query);

            return result.Match(
                departments => Ok(departments),
                errors => Problem(errors)
            );
        }

        // Remove Department
        [HttpDelete("{branchId:guid}/{departmentId:guid}")]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Remove(Guid branchId, Guid departmentId)
        {
            var command = new RemoveDepartmentCommand(branchId, departmentId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        [HttpGet("dropdown/{branchId:guid}")]
        public async Task<IActionResult> GetDropDown(Guid branchId)
        {
            var query = new GetDropDownDepartmentQuery(branchId);
            var result =
               await _mediator.Send(query);

            return result.Match(
                paged => Ok(paged),
                errors => Problem(errors)
            );
        }

        // Check if Department has Special Fields
        [HttpGet("{departmentId:guid}/special-fields")]
        public async Task<IActionResult> GetSpecialFields(Guid departmentId)
        {
            var query = new GetDepartmentSpecialFieldsQuery(departmentId);
            var result = await _mediator.Send(query);

            return result.Match(
                data => Ok(data),
                errors => Problem(errors)
            );
        }

    }
}
