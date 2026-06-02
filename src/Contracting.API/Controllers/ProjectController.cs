using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Master.Project.Command.CreateProject;
using Contracting.Application.Features.Master.Project.Command.DeleteProject;
using Contracting.Application.Features.Master.Project.Command.UpdateProject;
using Contracting.Application.Features.Master.Project.Command.UpdateProjectStatus;
using Contracting.Application.Features.Master.Project.Query.GetAllProjects;
using Contracting.Application.Features.Master.Project.Query.GetProjectById;
using Contracting.Application.Features.Master.Project.Query.GetProjectDropdown;
using Contracting.Shared.Common.Enums;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProjectController : APIBaseController
    {
        private readonly IMediator _mediator;

        public ProjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // Create Project
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Create([FromForm] CreateProjectDto dto)
        {
            var command = new CreateProjectCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                project => Ok(project),
                errors => Problem(errors)
            );
        }

        // Update Project
        [HttpPut]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Update([FromForm] UpdateProjectDto dto)
        {
            var command = new UpdateProjectCommand(dto);
            var result = await _mediator.Send(command);

            return result.Match(
                project => Ok(project),
                errors => Problem(errors)
            );
        }

        // Delete Project
        [HttpDelete("{projectId:guid}")]
        [Authorize(Roles = "SuperAdmin,IT")]
        public async Task<IActionResult> Delete(Guid projectId)
        {
            var command = new DeleteProjectCommand(projectId);
            var result = await _mediator.Send(command);

            return result.Match(
                success => Ok(success),
                errors => Problem(errors)
            );
        }

        // Get All Projects (Paginated)
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] Guid? branchId, [FromQuery] BaseFilterDto filter, [FromQuery] ProjectStatus? status = null)
        {
            var query = new GetAllProjectsQuery(branchId, filter, status);
            var result = await _mediator.Send(query);

            return result.Match(
                projects => Ok(projects),
                errors => Problem(errors)
            );
        }

        // Get Project By ID
        [HttpGet("{projectId:guid}")]
        public async Task<IActionResult> GetById(Guid projectId)
        {
            var query = new GetProjectByIdQuery(projectId);
            var result = await _mediator.Send(query);

            return result.Match(
                project => Ok(project),
                errors => Problem(errors)
            );
        }

        // Get Project Dropdown
        [HttpGet("dropdown")]
        public async Task<IActionResult> GetDropdown([FromQuery] Guid? branchId)
        {
            var query = new GetProjectDropdownQuery(branchId);
            var result = await _mediator.Send(query);

            return result.Match(
                projects => Ok(projects),
                errors => Problem(errors)
            );
        }

        // Update Project Status (with transition validation)
        [HttpPatch("{projectId:guid}/status")]
        public async Task<IActionResult> UpdateStatus(Guid projectId, [FromBody] ProjectStatus newStatus)
        {
            var command = new UpdateProjectStatusCommand(projectId, newStatus);
            var result = await _mediator.Send(command);

            return result.Match(
                project => Ok(project),
                errors => Problem(errors)
            );
        }
    }
}