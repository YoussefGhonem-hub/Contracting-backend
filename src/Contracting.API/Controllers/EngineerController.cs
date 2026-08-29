using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.Engineer.Command.CreateEngineer;
using Contracting.Application.Features.Master.Engineer.Command.DeleteEngineer;
using Contracting.Application.Features.Master.Engineer.Command.DeleteEngineerDepartment;
using Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerById;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerDropdown;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerList;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerListByBranch;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerProjects;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestCountByStatus;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerDepartments;
using Contracting.Application.Features.Master.Engineer.Query.GetEngineerSignature;
using Contracting.Application.Features.Master.Engineer.Command.UpdateEngineerProjectFeatures;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
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
        public async Task<IActionResult> GetList([FromQuery] Guid departmentId, [FromQuery] BaseFilterDto filter, [FromQuery] string? name = null, [FromQuery] string? email = null)
        {
            var query = new GetEngineerListQuery(departmentId, filter, name, email);
            var result = await _mediator.Send(query);

            return result.Match(
                engineers => Ok(engineers),
                errors => Problem(errors)
            );
        }

        // Get All Engineers (with filter)
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllEngineers([FromQuery] BaseFilterDto filter)
        {
            var query = new GetEngineerListByBranchQuery(filter);
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
        [HttpGet("dropdown/{departmentId:guid}")]
        public async Task<IActionResult> GetDropdown(Guid departmentId)
        {
            var query = new GetEngineerDropdownQuery(departmentId);
            var result = await _mediator.Send(query);

            return result.Match(
                engineers => Ok(engineers),
                errors => Problem(errors)
            );
        }

        // Get Engineer Request Count By Status
        [HttpGet("{engineerId:guid}/request-count-by-status")]
        public async Task<IActionResult> GetRequestCountByStatus(Guid engineerId)
        {
            var query = new GetEngineerRequestCountByStatusQuery(engineerId);
            var result = await _mediator.Send(query);

            return result.Match(
                counts => Ok(counts),
                errors => Problem(errors)
            );
        }

        // Get Engineer Signature (employeeId = Engineer.Id)
        [HttpGet("{engineerId:guid}/signature")]
        public async Task<IActionResult> GetSignature(Guid engineerId)
        {
            var query = new GetEngineerSignatureQuery(engineerId);
            var result = await _mediator.Send(query);

            return result.Match(
                signature => Ok(signature),
                errors => Problem(errors)
            );
        }

        // Get projects assigned to engineer (SuperAdmin gets all projects)
        [HttpGet("{engineerId:guid}/projects")]
        public async Task<IActionResult> GetEngineerProjects(Guid engineerId, [FromQuery] Guid? branchId = null)
        {
            var query = new GetEngineerProjectsQuery(engineerId, branchId);
            var result = await _mediator.Send(query);

            return result.Match(
                projects => Ok(projects),
                errors => Problem(errors)
            );
        }

        // Update feature permissions for an engineer on a specific project
        [HttpPut("{engineerId:guid}/projects/{projectId:guid}/features")]
        public async Task<IActionResult> UpdateProjectFeatures(
            Guid engineerId,
            Guid projectId,
            [FromBody] UpdateEngineerProjectFeaturesDto dto)
        {
            var command = new UpdateEngineerProjectFeaturesCommand(engineerId, projectId, dto.Features);
            var result = await _mediator.Send(command);

            return result.Match(
                project => Ok(project),
                errors => Problem(errors)
            );
        }

        // Get departments assigned to engineer (with roles)
        [HttpGet("{engineerId:guid}/departments")]
        public async Task<IActionResult> GetEngineerDepartments(Guid engineerId)
        {
            var query = new GetEngineerDepartmentsQuery(engineerId);
            var result = await _mediator.Send(query);

            return result.Match(
                departments => Ok(departments),
                errors => Problem(errors)
            );
        }

        // Delete department assignment from engineer
        [HttpDelete("{engineerId:guid}/departments/{departmentId:guid}")]
        public async Task<IActionResult> DeleteEngineerDepartment(Guid engineerId, Guid departmentId)
        {
            var command = new DeleteEngineerDepartmentCommand(engineerId, departmentId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }
    }
}
