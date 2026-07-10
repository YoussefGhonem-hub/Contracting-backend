using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Business.EngineerRequest.Command.ConfirmDeliveryDate;
using Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.CreateInternalRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.ReassignEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest;
using Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAllRequestsByDepartment;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetAllInternalRequests;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestsByFilter;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestActivities;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestById;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestCreatedOrApplyToEngineer;
using Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestsByStatusForEngineer;
using Contracting.Application.Features.Business.PurchaseRequest.Command.CreateGoodsReceipt;
using Contracting.Application.Features.Business.PurchaseRequest.Query.GetGoodsReceipts;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos;
using Contracting.Infrustructure.Persistence;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Contracting.Shared.CurrentUser;

namespace Contracting.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class EngineerRequestController : APIBaseController
    {
        private readonly IMediator _mediator;
        private readonly ApplicationDbContext _db;

        public EngineerRequestController(IMediator mediator, ApplicationDbContext db)
        {
            _mediator = mediator;
            _db = db;
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
        // Get all unified requests (EngineerRequest, TransferRequest, LaborAttendance, FinancialClearance)
        // Returns all request types created by or applied to the current engineer
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

        // =========================================================================
        // Internal Request — create
        // Office Engineers and Team Leads. Assigns directly to a peer in the same branch.
        // =========================================================================
        [HttpPost("internal")]
        [Authorize(Roles = RoleNames.Officeengineer + "," + RoleNames.SuperAdmin + "," + RoleNames.Teamleadengineer)]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateInternal([FromForm] CreateInternalRequestDto dto)
        {
            var result = await _mediator.Send(new CreateInternalRequestCommand(dto));
            return result.Match(r => Ok(r), errors => Problem(errors));
        }

        // =========================================================================
        // Internal Request — get list with full filtration
        // =========================================================================
        [HttpGet("internal")]
        public async Task<IActionResult> GetAllInternal([FromQuery] InternalRequestFilterDto filter)
        {
            var result = await _mediator.Send(new GetAllInternalRequestsQuery(filter));
            return Ok(result);
        }

        // =========================================================================
        // Internal Request — dropdown: departments in the logged-in engineer's branch
        // =========================================================================
        [HttpGet("internal/my-branch/departments")]
        [Authorize(Roles = RoleNames.Officeengineer + "," + RoleNames.SuperAdmin)]
        public async Task<IActionResult> GetMyBranchDepartments()
        {
            var userId = CurrentUser.Id ?? Guid.Empty;
            var isSuperAdmin = User.IsInRole(RoleNames.SuperAdmin);

            var branchId = await _db.Engineers
                .Where(e => e.ApplicationUserId == userId && !e.IsDeleted)
                .Select(e => e.Department != null ? (Guid?)e.Department.BranchId : null)
                .FirstOrDefaultAsync();

            if (branchId is null && !isSuperAdmin)
                return BadRequest(new { message = "Your account is not assigned to a branch." });

            var query = _db.Departmentes.Where(d => !d.IsDeleted);
            if (branchId.HasValue)
                query = query.Where(d => d.BranchId == branchId.Value);

            var departments = await query
                .OrderBy(d => d.nameEn)
                .Select(d => new { d.Id, d.nameEn, d.nameAr })
                .ToListAsync();

            return Ok(departments);
        }

        // =========================================================================
        // Internal Request — dropdown: engineers in a department (same branch only)
        // =========================================================================
        [HttpGet("internal/departments/{departmentId:guid}/engineers")]
        [Authorize(Roles = RoleNames.Officeengineer + "," + RoleNames.SuperAdmin)]
        public async Task<IActionResult> GetDepartmentEngineers(Guid departmentId)
        {
            var userId = CurrentUser.Id ?? Guid.Empty;
            var isSuperAdmin = User.IsInRole(RoleNames.SuperAdmin);

            // Resolve requester's branch — superadmin has no Engineer record, so skip branch check
            var requesterBranchId = await _db.Engineers
                .Where(e => e.ApplicationUserId == userId && !e.IsDeleted)
                .Select(e => e.Department != null ? (Guid?)e.Department.BranchId : null)
                .FirstOrDefaultAsync();

            if (requesterBranchId is null && !isSuperAdmin)
                return BadRequest(new { message = "Your account is not assigned to a branch." });

            // Ensure the department exists (and is in the same branch for non-superadmin)
            var deptBranchId = await _db.Departmentes
                .Where(d => d.Id == departmentId && !d.IsDeleted)
                .Select(d => (Guid?)d.BranchId)
                .FirstOrDefaultAsync();

            if (deptBranchId is null)
                return NotFound(new { message = "Department not found." });

            if (!isSuperAdmin && deptBranchId != requesterBranchId)
                return BadRequest(new { message = "Department does not belong to your branch." });

            // Engineers via EngineerDepartments join table
            var fromJoinTable = await _db.EngineerDepartments
                .Where(ed => ed.DepartmentId == departmentId && !ed.Engineer!.IsDeleted)
                .Select(ed => new { ed.Engineer!.Id, ed.Engineer.nameEn, ed.Engineer.nameAr, ed.Engineer.Email })
                .ToListAsync();

            // Engineers via legacy DepartmentId field
            var fromLegacy = await _db.Engineers
                .Where(e => e.DepartmentId == departmentId && !e.IsDeleted
                            && e.ApplicationUserId != userId)    // exclude self
                .Select(e => new { e.Id, e.nameEn, e.nameAr, e.Email })
                .ToListAsync();

            var engineers = fromJoinTable
                .Union(fromLegacy)
                .DistinctBy(e => e.Id)
                .Where(e => e.Id != Guid.Empty)
                .OrderBy(e => e.nameEn)
                .ToList();

            return Ok(engineers);
        }
    }
}