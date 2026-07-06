using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.business.enums;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business
{
    public class LaborAttendanceService : ILaborAttendanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;
        private readonly INotificationService _notificationService;

        public LaborAttendanceService(ApplicationDbContext db, Storage.AWS3.Services.IStorageService storageService, INotificationService notificationService)
        {
            _db = db;
            _storageService = storageService;
            _notificationService = notificationService;
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> CreateAsync(CreateLaborAttendanceRequestDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            if (string.IsNullOrEmpty(CurrentUser.UserId) || !Guid.TryParse(CurrentUser.UserId, out var currentUserId))
                return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId);

            if (dto.Records == null || dto.Records.Count == 0)
                return Error.Validation("LaborAttendance.RecordsRequired", "At least one labor record must be added.");

            var request = new LaborAttendanceRequest
            {
                RequestNumber = await GenerateRequestNumberAsync(),
                ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId,
                DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId,
                SiteName = dto.SiteName,
                AttendanceDate = dto.AttendanceDate,
                SupervisorId = engineer?.Id,
                Notes = dto.Notes,
                StatusId = s.New  // Pending → New
            };

            foreach (var rec in dto.Records)
            {
                if (!Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out var attendanceStatus))
                    return Error.Validation("LaborAttendance.InvalidStatus", $"Invalid attendance status: {rec.AttendanceStatus}");

                if (rec.DailyRate < 0)
                    return Error.Validation("LaborAttendance.NegativeRate", "Daily rate cannot be negative.");

                if (rec.OvertimeHours < 0)
                    return Error.Validation("LaborAttendance.NegativeOvertime", "Overtime hours cannot be negative.");

                if (attendanceStatus == WorkerAttendanceStatus.Absent && rec.OvertimeHours > 0)
                    return Error.Validation("LaborAttendance.AbsentWithOvertime", $"Worker '{rec.Name}' is absent but has overtime hours.");

                var totalAmount = CalculateTotalAmount(attendanceStatus, rec.DailyRate, rec.OvertimeHours);

                request.Records.Add(new LaborAttendanceRecord
                {
                    Name = rec.Name,
                    JobTitle = rec.JobTitle,
                    AttendanceStatus = attendanceStatus,
                    DailyRate = rec.DailyRate,
                    OvertimeHours = rec.OvertimeHours,
                    TotalAmount = totalAmount,
                    Notes = rec.Notes
                });
            }

            var names = request.Records.Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .Select(r => r.Name!.Trim().ToLower()).ToList();
            if (names.Count != names.Distinct().Count())
                return Error.Validation("LaborAttendance.DuplicateRecord", "Duplicate labor records are not allowed on the same request.");

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                    foreach (var f in uploaded)
                        request.Attachments.Add(new LaborAttendanceAttachment
                        {
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            request.Activities.Add(new LaborAttendanceActivity
            {
                EngineerId = engineer?.Id,
                ToStatusId = s.New,
                ActionType = "Created"
            });

            await _db.LaborAttendanceRequests.AddAsync(request);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> UpdateAsync(UpdateLaborAttendanceRequestDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Records)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            // Only allow editing when Pending (New)
            if (request.StatusId != s.New)
                return Error.Validation("LaborAttendance.CannotEdit", "Only Pending requests can be edited.");

            if (dto.ProjectId.HasValue) request.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            if (dto.DepartmentId.HasValue) request.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            if (dto.SiteName is not null) request.SiteName = dto.SiteName;
            if (dto.AttendanceDate.HasValue) request.AttendanceDate = dto.AttendanceDate.Value;
            if (dto.Notes is not null) request.Notes = dto.Notes;

            if (dto.Records != null)
            {
                _db.LaborAttendanceRecords.RemoveRange(request.Records);
                foreach (var rec in dto.Records)
                {
                    if (!Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out var attendanceStatus))
                        return Error.Validation("LaborAttendance.InvalidStatus", $"Invalid attendance status: {rec.AttendanceStatus}");
                    if (rec.DailyRate < 0)
                        return Error.Validation("LaborAttendance.NegativeRate", "Daily rate cannot be negative.");
                    if (rec.OvertimeHours < 0)
                        return Error.Validation("LaborAttendance.NegativeOvertime", "Overtime hours cannot be negative.");
                    if (attendanceStatus == WorkerAttendanceStatus.Absent && rec.OvertimeHours > 0)
                        return Error.Validation("LaborAttendance.AbsentWithOvertime", $"Worker '{rec.Name}' is absent but has overtime hours.");

                    request.Records.Add(new LaborAttendanceRecord
                    {
                        LaborAttendanceRequestId = request.Id,
                        Name = rec.Name,
                        JobTitle = rec.JobTitle,
                        AttendanceStatus = attendanceStatus,
                        DailyRate = rec.DailyRate,
                        OvertimeHours = rec.OvertimeHours,
                        TotalAmount = CalculateTotalAmount(attendanceStatus, rec.DailyRate, rec.OvertimeHours),
                        Notes = rec.Notes
                    });
                }
            }

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                    foreach (var f in uploaded)
                        request.Attachments.Add(new LaborAttendanceAttachment
                        {
                            LaborAttendanceRequestId = request.Id,
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var request = await _db.LaborAttendanceRequests.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            // Only allow deleting when Pending (New)
            if (request.StatusId != s.New)
                return Error.Validation("LaborAttendance.CannotDelete", "Only Pending requests can be deleted.");

            request.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult("Deleted successfully.");
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdAsync(Guid id)
        {
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Supervisor)
                .Include(r => r.AssignedTo)
                .Include(r => r.Status)
                .Include(r => r.Records)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
                .Include(r => r.Activities).ThenInclude(a => a.FromStatus)
                .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            return MapToDto(request);
        }

        public async Task<PaginatedList<GetLaborAttendanceRequestDto>> GetAllAsync(LaborAttendanceFilterDto filter)
        {
            try
            {
                var roles = CurrentUser.Roles;
                var isAdmin = roles.Any(r => r.Equals(Contracting.Shared.Constants.RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                          || r.Equals(Contracting.Shared.Constants.RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

                var query = _db.LaborAttendanceRequests
                    .Include(r => r.Project)
                    .Include(r => r.Department)
                    .Include(r => r.Supervisor)
                    .Include(r => r.AssignedTo)
                    .Include(r => r.Status)
                    .Include(r => r.Records)
                    .Include(r => r.Attachments)
                    .Include(r => r.Activities).ThenInclude(a => a.Engineer)
                    .Include(r => r.Activities).ThenInclude(a => a.FromStatus)
                    .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
                    .Where(r => !r.IsDeleted)
                    .AsNoTracking();

                if (!isAdmin && Guid.TryParse(CurrentUser.UserId, out var currentUserId))
                {
                    var engineer = await _db.Engineers.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId);

                    if (engineer != null)
                    {
                        var teamLeadDeptIds = await (from ed in _db.EngineerDepartments
                                                     join role in _db.Roles on ed.RoleId equals role.Id
                                                     where ed.EngineerId == engineer.Id && role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer
                                                     select ed.DepartmentId)
                                                    .ToListAsync();

                        if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
                        {
                            var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                            join role in _db.Roles on userRole.RoleId equals role.Id
                                                            where userRole.UserId == engineer.ApplicationUserId
                                                                  && role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer
                                                            select role.Id).AnyAsync();
                            if (isTeamLeadViaRoles)
                                teamLeadDeptIds.Add(engineer.DepartmentId.Value);
                        }

                        if (teamLeadDeptIds.Any())
                        {
                            query = query.Where(r =>
                                (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                                || r.AssignedToId == engineer.Id
                                || r.SupervisorId == engineer.Id);
                        }
                        else
                        {
                            query = query.Where(r => r.SupervisorId == engineer.Id || r.AssignedToId == engineer.Id);
                        }
                    }
                }

                // Resolve the status filter. Callers may pass either an explicit
                // StatusId (Guid) or a status name/code via ?status=... (e.g. "Rejected").
                // The latter was previously unbound, so the filter was silently ignored
                // and records of every status (e.g. Completed) were returned.
                var statusId = filter.StatusId;
                if (!statusId.HasValue && !string.IsNullOrWhiteSpace(filter.Status))
                {
                    var normalized = filter.Status.Trim().ToUpper();
                    statusId = await _db.Statuses
                        .AsNoTracking()
                        .Where(st => (st.Code != null && st.Code.ToUpper() == normalized)
                                     || (st.nameEn != null && st.nameEn.ToUpper() == normalized)
                                     || (st.nameAr != null && st.nameAr.ToUpper() == normalized))
                        .Select(st => (Guid?)st.Id)
                        .FirstOrDefaultAsync();

                    // Unknown status value: return an empty page rather than silently
                    // ignoring the filter and leaking records of other statuses.
                    if (!statusId.HasValue)
                        return new PaginatedList<GetLaborAttendanceRequestDto>(
                            new List<GetLaborAttendanceRequestDto>(), 0, filter.PageIndex, filter.PageSize);
                }

                if (statusId.HasValue)
                    query = query.Where(r => r.StatusId == statusId);

                if (filter.ProjectId.HasValue) query = query.Where(r => r.ProjectId == filter.ProjectId);
                if (filter.SupervisorId.HasValue) query = query.Where(r => r.SupervisorId == filter.SupervisorId);
                if (filter.FromDate.HasValue) query = query.Where(r => r.AttendanceDate >= filter.FromDate);
                if (filter.ToDate.HasValue) query = query.Where(r => r.AttendanceDate <= filter.ToDate);
                if (!string.IsNullOrWhiteSpace(filter.Search))
                    query = query.Where(r => r.RequestNumber!.Contains(filter.Search) || r.SiteName!.Contains(filter.Search));

                var totalCount = await query.CountAsync();
                var items = await query
                    .OrderByDescending(r => r.CreatedDate)
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                return new PaginatedList<GetLaborAttendanceRequestDto>(
                    items.Select(MapToDto).ToList(),
                    totalCount, filter.PageIndex, filter.PageSize);
            }
            catch (Exception)
            {
                return new PaginatedList<GetLaborAttendanceRequestDto>(
                    new List<GetLaborAttendanceRequestDto>(), 0, filter.PageIndex, filter.PageSize);
            }
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> TakeActionAsync(Guid id, LaborAttendanceActionDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Records)
                .Include(r => r.Supervisor)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");

            if (string.IsNullOrEmpty(CurrentUser.UserId) || !Guid.TryParse(CurrentUser.UserId, out var currentUserId))
                return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId);

            Domain.Entities.master.Engineer? assignedEngineer = null;
            var fromStatusId = request.StatusId;
            Guid toStatusId;

            switch (dto.ActionType.ToLower())
            {
                case "assign":
                    // Pending (New) → Assigned (InProgress)
                    if (request.StatusId != s.New)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Pending requests can be assigned.");
                    if (!dto.AssignedToId.HasValue || dto.AssignedToId == Guid.Empty)
                        return Error.Validation("LaborAttendance.AssignedToRequired", "AssignedToId is required for assign action.");

                    assignedEngineer = await _db.Engineers.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId.Value);
                    if (assignedEngineer is null)
                        return Error.NotFound("LaborAttendance.EngineerNotFound", "Assigned engineer not found.");

                    toStatusId = s.InProgress;
                    break;
                case "validate":
                    // Assigned (InProgress) → Validated (Completed)
                    if (request.StatusId != s.InProgress)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Assigned requests can be validated.");
                    toStatusId = s.Completed;
                    break;
                case "reject":
                    // Assigned (InProgress) → Rejected
                    if (request.StatusId != s.InProgress)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Assigned requests can be rejected.");
                    toStatusId = s.Rejected;
                    break;
                default:
                    return Error.Validation("LaborAttendance.UnknownAction", $"Unknown action: {dto.ActionType}");
            }

            if (assignedEngineer is not null)
            {
                await _db.LaborAttendanceRequests
                    .Where(r => r.Id == id)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(r => r.StatusId, toStatusId)
                        .SetProperty(r => r.AssignedToId, assignedEngineer.Id));
            }
            else
            {
                await _db.LaborAttendanceRequests
                    .Where(r => r.Id == id)
                    .ExecuteUpdateAsync(s => s.SetProperty(r => r.StatusId, toStatusId));
            }

            await _db.LaborAttendanceActivities.AddAsync(new LaborAttendanceActivity
            {
                LaborAttendanceRequestId = id,
                EngineerId = engineer?.Id,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId,
                ActionType = dto.ActionType,
                Comments = dto.Comments
            });
            await _db.SaveChangesAsync();

            var supervisorUserId = request.Supervisor?.ApplicationUserId;
            switch (dto.ActionType.ToLower())
            {
                case "assign":
                    await _notificationService.SendNotificationToUserAsync(
                        assignedEngineer!.ApplicationUserId,
                        "Labor Attendance Request Assigned",
                        $"Request {request.RequestNumber} has been assigned to you for review.",
                        id);
                    break;
                case "validate":
                    if (supervisorUserId.HasValue)
                        await _notificationService.SendNotificationToUserAsync(
                            supervisorUserId.Value,
                            "Labor Attendance Request Validated",
                            $"Your request {request.RequestNumber} has been validated.",
                            id);

                    // Fan-out: notify all engineers in departments with NotifyAfterLaborApprove in this branch
                    Guid? branchId = null;
                    if (request.ProjectId.HasValue)
                        branchId = await _db.Projects
                            .Where(p => p.Id == request.ProjectId.Value)
                            .Select(p => (Guid?)p.BranchId)
                            .FirstOrDefaultAsync();
                    if (!branchId.HasValue && request.DepartmentId.HasValue)
                        branchId = await _db.Departmentes
                            .Where(d => d.Id == request.DepartmentId.Value)
                            .Select(d => (Guid?)d.BranchId)
                            .FirstOrDefaultAsync();

                    if (branchId.HasValue)
                    {
                        var notifyDeptIds = await _db.Departmentes
                            .Where(d => d.BranchId == branchId.Value && d.NotifyAfterLaborApprove && !d.IsDeleted)
                            .Select(d => d.Id)
                            .ToListAsync();

                        foreach (var deptId in notifyDeptIds)
                        {
                            var fromJoin = await _db.EngineerDepartments
                                .Where(ed => ed.DepartmentId == deptId && !ed.Engineer!.IsDeleted)
                                .Select(ed => ed.Engineer!.ApplicationUserId).ToListAsync();
                            var fromLegacy = await _db.Engineers
                                .Where(e => e.DepartmentId == deptId && !e.IsDeleted)
                                .Select(e => e.ApplicationUserId).ToListAsync();
                            var recipients = fromJoin.Union(fromLegacy)
                                .Where(uid => uid != Guid.Empty).Distinct();

                            foreach (var recipientUserId in recipients)
                                await _notificationService.SendNotificationToUserAsync(
                                    recipientUserId,
                                    "Labor Attendance Request Ready for Processing",
                                    $"Labor request {request.RequestNumber} has been validated and is ready for processing.",
                                    id, deptId, null, "LaborAttendance");
                        }
                    }
                    break;
                case "reject":
                    if (supervisorUserId.HasValue)
                        await _notificationService.SendNotificationToUserAsync(
                            supervisorUserId.Value,
                            "Labor Attendance Request Rejected",
                            $"Your request {request.RequestNumber} has been rejected. {dto.Comments}",
                            id);
                    break;
            }

            return await GetByIdAsync(id);
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> ReassignAsync(Guid id, ReassignLaborAttendanceDto dto)
        {
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Supervisor)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null)
                return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");

            if (string.IsNullOrEmpty(CurrentUser.UserId) || !Guid.TryParse(CurrentUser.UserId, out var currentUserId))
                return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

            if (dto.AssignedToId == Guid.Empty)
                return Error.Validation("LaborAttendance.AssignedToRequired", "AssignedToId is required.");

            var newAssignee = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId);
            if (newAssignee is null)
                return Error.NotFound("LaborAttendance.EngineerNotFound", "Assigned engineer not found.");

            if (request.AssignedToId == dto.AssignedToId)
                return Error.Validation("LaborAttendance.AlreadyAssigned", "The request is already assigned to this engineer.");

            var actingEngineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId);

            // Reassign only changes the assignee; the status is kept as-is.
            await _db.LaborAttendanceRequests
                .Where(r => r.Id == id)
                .ExecuteUpdateAsync(u => u.SetProperty(r => r.AssignedToId, dto.AssignedToId));

            var currentStatusId = request.StatusId ?? Guid.Empty;
            await _db.LaborAttendanceActivities.AddAsync(new LaborAttendanceActivity
            {
                LaborAttendanceRequestId = id,
                EngineerId = actingEngineer?.Id,
                FromStatusId = currentStatusId,
                ToStatusId = currentStatusId,
                ActionType = "reassign",
                Comments = dto.Comments
            });
            await _db.SaveChangesAsync();

            await _notificationService.SendNotificationToUserAsync(
                newAssignee.ApplicationUserId,
                "Labor Attendance Request Reassigned",
                $"Request {request.RequestNumber} has been reassigned to you.",
                id);

            return await GetByIdAsync(id);
        }

        private static decimal CalculateTotalAmount(WorkerAttendanceStatus status, decimal dailyRate, decimal overtimeHours)
        {
            return status switch
            {
                WorkerAttendanceStatus.Present => dailyRate,
                WorkerAttendanceStatus.HalfDay => dailyRate * 0.5m,
                WorkerAttendanceStatus.Overtime => dailyRate + (overtimeHours * (dailyRate / 8m)),
                WorkerAttendanceStatus.Absent => 0m,
                _ => 0m
            };
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _db.LaborAttendanceRequests.CountAsync(r => r.CreatedDate.Year == year);
            return $"LAB-{year}-{(count + 1):D5}";
        }

        private static GetDropDownStatusDto? MapStatus(Status? s) => s is null ? null : new GetDropDownStatusDto
        {
            Id = s.Id,
            nameEn = s.nameEn,
            nameAr = s.nameAr,
            Code = s.Code,
            orderNumber = s.orderNumber,
            iconName = s.iconName
        };

        private GetLaborAttendanceRequestDto MapToDto(LaborAttendanceRequest r) => new()
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            ProjectId = r.ProjectId,
            Project = r.Project is null ? null : new GetProjectDto { Id = r.Project.Id, nameEn = r.Project.nameEn, nameAr = r.Project.nameAr },
            DepartmentId = r.DepartmentId,
            Department = r.Department is null ? null : new GetDepartmentDto { Id = r.Department.Id, nameEn = r.Department.nameEn, nameAr = r.Department.nameAr },
            SiteName = r.SiteName,
            AttendanceDate = r.AttendanceDate,
            SupervisorId = r.SupervisorId,
            Supervisor = r.Supervisor is null ? null : new GetEngineerDto { Id = r.Supervisor.Id, nameEn = r.Supervisor.nameEn, nameAr = r.Supervisor.nameAr },
            AssignedToId = r.AssignedToId,
            AssignedTo = r.AssignedTo is null ? null : new GetEngineerDto { Id = r.AssignedTo.Id, nameEn = r.AssignedTo.nameEn, nameAr = r.AssignedTo.nameAr },
            Notes = r.Notes,
            StatusId = r.StatusId,
            Status = MapStatus(r.Status),
            TotalAmount = r.Records.Sum(rec => rec.TotalAmount),
            CreatedDate = r.CreatedDate,
            Records = r.Records.Select(rec => new GetLaborAttendanceRecordDto
            {
                Id = rec.Id,
                Name = rec.Name,
                JobTitle = rec.JobTitle,
                AttendanceStatus = rec.AttendanceStatus.ToString(),
                DailyRate = rec.DailyRate,
                OvertimeHours = rec.OvertimeHours,
                TotalAmount = rec.TotalAmount,
                Notes = rec.Notes
            }).ToList(),
            Attachments = r.Attachments.Select(a => new GetAttachmentDto
            {
                Id = a.Id, Key = a.Key, FileName = a.FileName,
                Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList(),
            Activities = r.Activities.Select(a => new GetLaborAttendanceActivityDto
            {
                Id = a.Id,
                FromStatusId = a.FromStatusId,
                FromStatus = MapStatus(a.FromStatus),
                ToStatusId = a.ToStatusId,
                ToStatus = MapStatus(a.ToStatus),
                ActionType = a.ActionType,
                Comments = a.Comments,
                Engineer = a.Engineer is null ? null : new GetEngineerDto { Id = a.Engineer.Id, nameEn = a.Engineer.nameEn, nameAr = a.Engineer.nameAr },
                CreatedDate = a.CreatedDate
            }).ToList(),
            EngineerRequestNotes = r.Activities
                .Where(a => !string.IsNullOrWhiteSpace(a.Comments))
                .Select(a => new GetEngineerRequestNotesDto
                {
                    Id          = a.Id,
                    note        = a.Comments,
                    EngineerId  = a.EngineerId,
                    Engineer    = a.Engineer is null ? null : new GetEngineerDto { Id = a.Engineer.Id, nameEn = a.Engineer.nameEn, nameAr = a.Engineer.nameAr },
                    CreatedDate = a.CreatedDate,
                    Attachments = new List<GetAttachmentDto>()
                })
                .OrderBy(n => n.CreatedDate)
                .ToList()
        };
    }
}
