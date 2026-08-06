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
using Microsoft.Extensions.Logging;

namespace Contracting.Infrustructure.Features.business
{
    public class LaborAttendanceService : ILaborAttendanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<LaborAttendanceService> _logger;

        public LaborAttendanceService(ApplicationDbContext db, Storage.AWS3.Services.IStorageService storageService, INotificationService notificationService, ILogger<LaborAttendanceService> logger)
        {
            _db = db;
            _storageService = storageService;
            _notificationService = notificationService;
            _logger = logger;
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

            // Notify the team lead of the selected department (or all department members if none).
            try
            {
                var deptId = request.DepartmentId;
                if (deptId.HasValue)
                {
                    var teamLeadUserId = await _db.EngineerDepartments
                        .Where(ed => ed.DepartmentId == deptId.Value
                                  && ed.Role != null
                                  && ed.Role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer)
                        .Select(ed => ed.Engineer!.ApplicationUserId)
                        .FirstOrDefaultAsync();

                    if (teamLeadUserId == Guid.Empty)
                    {
                        teamLeadUserId = await (from eng in _db.Engineers
                                                join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                                join role in _db.Roles on userRole.RoleId equals role.Id
                                                where eng.DepartmentId == deptId.Value
                                                      && role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer
                                                select eng.ApplicationUserId)
                                           .FirstOrDefaultAsync();
                    }

                    if (teamLeadUserId != Guid.Empty)
                    {
                        await _notificationService.SendNotificationToUserAsync(
                            teamLeadUserId,
                            "New Labor Attendance Request",
                            $"A new labor attendance request {request.RequestNumber} has been submitted.",
                            request.Id,
                            deptId);
                    }
                    else
                    {
                        var joinIds = await _db.EngineerDepartments
                            .Where(ed => ed.DepartmentId == deptId.Value && !ed.Engineer!.IsDeleted)
                            .Select(ed => ed.Engineer!.ApplicationUserId).ToListAsync();
                        var legacyIds = await _db.Engineers
                            .Where(e => e.DepartmentId == deptId.Value && !e.IsDeleted)
                            .Select(e => e.ApplicationUserId).ToListAsync();
                        foreach (var uid in joinIds.Union(legacyIds).Where(u => u != Guid.Empty).Distinct())
                            await _notificationService.SendNotificationToUserAsync(
                                uid,
                                "New Labor Attendance Request",
                                $"A new labor attendance request {request.RequestNumber} has been submitted.",
                                request.Id,
                                deptId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LaborAttendance {RequestId}: failed to send creation notification.", request.Id);
            }

            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> UpdateAsync(UpdateLaborAttendanceRequestDto dto)
        {
            try
            {
                return await UpdateInternalAsync(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LaborAttendance {RequestId}: unhandled exception in UpdateAsync.", dto.Id);
                return Error.Failure("LaborAttendance.Error", ex.InnerException?.Message ?? ex.Message);
            }
        }

        private async Task<ErrorOr<GetLaborAttendanceRequestDto>> UpdateInternalAsync(UpdateLaborAttendanceRequestDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);

            // Load without tracking — used only for validation; all writes go through
            // ExecuteUpdateAsync / direct DbSet.Add so SaveChangesAsync never needs to
            // UPDATE the request row (eliminates DbUpdateConcurrencyException on hosted SQL).
            var request = await _db.LaborAttendanceRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            // Allow editing in New (Draft) state, and in MissingInformation state so the
            // requester can fix/add the missing details before resubmitting.
            if (request.StatusId != s.New && request.StatusId != s.MissingInformation)
                return Error.Validation("LaborAttendance.CannotEdit", "Only new (draft) or missing-information requests can be edited.");

            bool wasInMissingInfo = request.StatusId == s.MissingInformation;

            // Compute final scalar values from the loaded (no-tracking) snapshot + DTO overrides.
            var finalProjectId    = dto.ProjectId.HasValue    ? (dto.ProjectId == Guid.Empty ? null : dto.ProjectId)       : request.ProjectId;
            var finalDepartmentId = dto.DepartmentId.HasValue ? (dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId) : request.DepartmentId;
            var finalSiteName     = dto.SiteName     ?? request.SiteName;
            var finalDate         = dto.AttendanceDate ?? request.AttendanceDate;
            var finalNotes        = dto.Notes         ?? request.Notes;

            bool hasScalarChanges =
                finalProjectId != request.ProjectId || finalDepartmentId != request.DepartmentId ||
                finalSiteName != request.SiteName   || finalDate != request.AttendanceDate ||
                finalNotes != request.Notes;

            // When resubmitting from MissingInformation always run ExecuteUpdate (status must change
            // even if no other scalar field changed); otherwise only run if something actually changed.
            if (hasScalarChanges || wasInMissingInfo)
            {
                await _db.LaborAttendanceRequests
                    .Where(r => r.Id == dto.Id)
                    .ExecuteUpdateAsync(u => u
                        .SetProperty(r => r.ProjectId,      finalProjectId)
                        .SetProperty(r => r.DepartmentId,   finalDepartmentId)
                        .SetProperty(r => r.SiteName,       finalSiteName)
                        .SetProperty(r => r.AttendanceDate, finalDate)
                        .SetProperty(r => r.Notes,          finalNotes)
                        .SetProperty(r => r.StatusId,       wasInMissingInfo ? s.New : request.StatusId));
            }

            if (dto.Records != null)
            {
                // Validate all incoming records before touching the database.
                foreach (var rec in dto.Records)
                {
                    if (!Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out _))
                        return Error.Validation("LaborAttendance.InvalidStatus", $"Invalid attendance status: {rec.AttendanceStatus}");
                    if (rec.DailyRate < 0)
                        return Error.Validation("LaborAttendance.NegativeRate", "Daily rate cannot be negative.");
                    if (rec.OvertimeHours < 0)
                        return Error.Validation("LaborAttendance.NegativeOvertime", "Overtime hours cannot be negative.");
                    if (Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out var ats)
                        && ats == WorkerAttendanceStatus.Absent && rec.OvertimeHours > 0)
                        return Error.Validation("LaborAttendance.AbsentWithOvertime", $"Worker '{rec.Name}' is absent but has overtime hours.");
                }

                var names = dto.Records.Where(r => !string.IsNullOrWhiteSpace(r.Name))
                    .Select(r => r.Name!.Trim().ToLower()).ToList();
                if (names.Count != names.Distinct().Count())
                    return Error.Validation("LaborAttendance.DuplicateRecord", "Duplicate labor records are not allowed.");

                // Hard-delete existing records directly — bypasses soft-delete tracking.
                await _db.LaborAttendanceRecords
                    .IgnoreQueryFilters()
                    .Where(r => r.LaborAttendanceRequestId == dto.Id)
                    .ExecuteDeleteAsync();

                foreach (var rec in dto.Records)
                {
                    Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out var attendanceStatus);
                    _db.LaborAttendanceRecords.Add(new LaborAttendanceRecord
                    {
                        LaborAttendanceRequestId = dto.Id,
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
                        _db.LaborAttendanceAttachments.Add(new LaborAttendanceAttachment
                        {
                            LaborAttendanceRequestId = dto.Id,
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            // When resubmitting from MissingInformation, add an activity record so the
            // audit trail shows the status transition back to New (Pending).
            if (wasInMissingInfo)
            {
                var engineer = await _db.Engineers.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ApplicationUserId == (CurrentUser.Id ?? Guid.Empty));
                _db.LaborAttendanceActivities.Add(new LaborAttendanceActivity
                {
                    LaborAttendanceRequestId = dto.Id,
                    EngineerId               = engineer?.Id,
                    FromStatusId             = s.MissingInformation,
                    ToStatusId               = s.New,
                    ActionType               = "Resubmit"
                });
            }

            // SaveChangesAsync persists: new records / attachments (Added) + resubmit activity (Added).
            // No tracked Modified request entity → no UPDATE row-count check → no concurrency exception.
            if (dto.Records != null || (dto.Attachments != null && dto.Attachments.Any()) || wasInMissingInfo)
                await _db.SaveChangesAsync();

            // Notify the team lead that the site engineer answered the missing-information
            // request, so it doesn't sit unnoticed back in the New/Pending queue.
            if (wasInMissingInfo)
                await NotifyTeamLeadOfResubmitAsync(dto.Id, request.RequestNumber, finalDepartmentId);

            return await GetByIdAsync(dto.Id);
        }

        private async Task NotifyTeamLeadOfResubmitAsync(Guid requestId, string? requestNumber, Guid? departmentId)
        {
            try
            {
                if (!departmentId.HasValue) return;

                var teamLeadUserId = await _db.EngineerDepartments
                    .Where(ed => ed.DepartmentId == departmentId.Value
                              && ed.Role != null
                              && ed.Role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer)
                    .Select(ed => ed.Engineer!.ApplicationUserId)
                    .FirstOrDefaultAsync();

                if (teamLeadUserId == Guid.Empty)
                {
                    teamLeadUserId = await (from eng in _db.Engineers
                                            join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                            join role in _db.Roles on userRole.RoleId equals role.Id
                                            where eng.DepartmentId == departmentId.Value
                                                  && role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer
                                            select eng.ApplicationUserId)
                                       .FirstOrDefaultAsync();
                }

                if (teamLeadUserId != Guid.Empty)
                {
                    await _notificationService.SendNotificationToUserAsync(
                        teamLeadUserId,
                        "Labor Attendance Request Updated",
                        $"Request {requestNumber} was updated with the missing information you requested and is ready for review.",
                        requestId,
                        departmentId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "LaborAttendance {RequestId}: failed to send resubmit notification to team lead.", requestId);
            }
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

        public Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdAsync(Guid id)
            => GetByIdCoreAsync(id, applyVisibility: true);

        // Used by the anonymous printable-report endpoint: the visibility filter
        // reads CurrentUser and would return an empty set for anonymous callers.
        // Access control for this path is the unguessable request GUID.
        public Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdForPublicReportAsync(Guid id)
            => GetByIdCoreAsync(id, applyVisibility: false);

        private async Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdCoreAsync(Guid id, bool applyVisibility)
        {
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
                .Where(r => r.Id == id && !r.IsDeleted)
                .AsNoTracking();

            if (applyVisibility)
                query = await ApplyLaborAttendanceVisibilityAsync(query);
            var request = await query.FirstOrDefaultAsync();

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            return MapToDto(request);
        }

        private async Task<bool> DepartmentHasTeamLeadAsync(Guid departmentId)
        {
            var teamleadRoleName = Contracting.Shared.Constants.RoleNames.Teamleadengineer;

            var hasViaDepRole = await _db.EngineerDepartments
                .AnyAsync(ed => ed.DepartmentId == departmentId
                             && ed.Role != null && ed.Role.Name == teamleadRoleName);
            if (hasViaDepRole) return true;

            return await (from eng in _db.Engineers
                          join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                          join role in _db.Roles on userRole.RoleId equals role.Id
                          where eng.DepartmentId == departmentId && role.Name == teamleadRoleName
                          select eng.Id).AnyAsync();
        }

        private async Task<bool> IsEngineerTeamLeadOfDepartmentAsync(Guid engineerId, Guid departmentId)
        {
            var teamleadRoleName = Contracting.Shared.Constants.RoleNames.Teamleadengineer;

            var isTeamLeadViaDeptRole = await _db.EngineerDepartments
                .AnyAsync(ed => ed.EngineerId == engineerId
                             && ed.DepartmentId == departmentId
                             && ed.Role != null && ed.Role.Name == teamleadRoleName);
            if (isTeamLeadViaDeptRole) return true;

            return await (from eng in _db.Engineers
                          join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                          join role in _db.Roles on userRole.RoleId equals role.Id
                          where eng.Id == engineerId && eng.DepartmentId == departmentId && role.Name == teamleadRoleName
                          select eng.Id).AnyAsync();
        }

        // When a department has no team lead, there is nobody to run the "assign" step, so a request can
        // sit in New/Pending forever. Lets any engineer belonging to that department claim + act on it
        // directly (validate/reject) in one step, instead of requiring assign first.
        private async Task<bool> CanAutoClaimNoTeamLeadRequestAsync(Guid? departmentId, Guid engineerId)
        {
            if (!departmentId.HasValue) return false;

            if (await DepartmentHasTeamLeadAsync(departmentId.Value)) return false;

            return await _db.EngineerDepartments
                .AnyAsync(ed => ed.EngineerId == engineerId && ed.DepartmentId == departmentId.Value)
                || await _db.Engineers.AnyAsync(e => e.Id == engineerId && e.DepartmentId == departmentId.Value);
        }

        // Applies the "department without a team lead → visible to the whole department until claimed"
        // rule to a labor-attendance query. Admins and department team leads see everything in their
        // scope; other engineers only see unassigned requests in team-lead-less departments they belong
        // to, plus anything already assigned to or supervised by them. Mirrors
        // EngineerRequestService.ApplyEngineerRequestVisibilityAsync.
        private async Task<IQueryable<LaborAttendanceRequest>> ApplyLaborAttendanceVisibilityAsync(
            IQueryable<LaborAttendanceRequest> query)
        {
            var roles = CurrentUser.Roles;
            var isAdmin = roles.Any(r => r.Equals(Contracting.Shared.Constants.RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                      || r.Equals(Contracting.Shared.Constants.RoleNames.Admin, StringComparison.OrdinalIgnoreCase));
            if (isAdmin) return query;

            if (!Guid.TryParse(CurrentUser.UserId, out var currentUserId))
                return query.Where(r => false);

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId);

            if (engineer == null)
                return query.Where(r => false);

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
                return query.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.AssignedToId == engineer.Id
                    || r.SupervisorId == engineer.Id);
            }

            var allEngineerDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineer.Id)
                .Select(ed => ed.DepartmentId)
                .ToListAsync();

            if (engineer.DepartmentId.HasValue && !allEngineerDeptIds.Contains(engineer.DepartmentId.Value))
                allEngineerDeptIds.Add(engineer.DepartmentId.Value);

            if (!allEngineerDeptIds.Any())
                return query.Where(r => r.SupervisorId == engineer.Id || r.AssignedToId == engineer.Id);

            var deptIdsWithTeamLead = await (from ed in _db.EngineerDepartments
                                             join role in _db.Roles on ed.RoleId equals role.Id
                                             where allEngineerDeptIds.Contains(ed.DepartmentId)
                                                   && role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer
                                             select ed.DepartmentId)
                                            .Distinct()
                                            .ToListAsync();

            var legacyTeamLeadDeptIds = await (from eng in _db.Engineers
                                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                               join role in _db.Roles on userRole.RoleId equals role.Id
                                               where allEngineerDeptIds.Contains(eng.DepartmentId ?? Guid.Empty)
                                                     && role.Name == Contracting.Shared.Constants.RoleNames.Teamleadengineer
                                               select eng.DepartmentId!.Value)
                                              .Distinct()
                                              .ToListAsync();

            foreach (var id2 in legacyTeamLeadDeptIds.Where(id2 => !deptIdsWithTeamLead.Contains(id2)))
                deptIdsWithTeamLead.Add(id2);

            var deptsWithoutTeamLead = allEngineerDeptIds.Where(d => !deptIdsWithTeamLead.Contains(d)).ToList();
            var deptsWithTeamLead = deptIdsWithTeamLead.Where(d => allEngineerDeptIds.Contains(d)).ToList();

            return query.Where(r =>
                (r.DepartmentId.HasValue && deptsWithoutTeamLead.Contains(r.DepartmentId.Value)
                    && (r.AssignedToId == null || r.AssignedToId == Guid.Empty || r.AssignedToId == engineer.Id))
                || (r.DepartmentId.HasValue && deptsWithTeamLead.Contains(r.DepartmentId.Value) && r.AssignedToId == engineer.Id)
                || r.AssignedToId == engineer.Id
                || r.SupervisorId == engineer.Id);
        }

        public async Task<PaginatedList<GetLaborAttendanceRequestDto>> GetAllAsync(LaborAttendanceFilterDto filter)
        {
            try
            {
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

                query = await ApplyLaborAttendanceVisibilityAsync(query);

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

                    if (request.DepartmentId.HasValue && engineer != null)
                    {
                        var departmentId = request.DepartmentId.Value;
                        var isManager = await IsEngineerTeamLeadOfDepartmentAsync(engineer.Id, departmentId);

                        if (!isManager)
                        {
                            var departmentHasTeamLead = await DepartmentHasTeamLeadAsync(departmentId);
                            if (departmentHasTeamLead)
                                return Error.Unauthorized("LaborAttendance.Unauthorized", "Only the department team lead can assign this request.");

                            var isEngineerInDepartment = await _db.EngineerDepartments
                                .AnyAsync(ed => ed.EngineerId == engineer.Id && ed.DepartmentId == departmentId)
                                || engineer.DepartmentId == departmentId;
                            if (!isEngineerInDepartment)
                                return Error.Unauthorized("LaborAttendance.Unauthorized", "You are not authorized to assign this request.");

                            // No team lead in this department: a normal engineer may only claim the request for themselves.
                            if (dto.AssignedToId.Value != engineer.Id)
                                return Error.Unauthorized("LaborAttendance.Unauthorized", "This department has no team lead — you can only assign this request to yourself.");
                        }
                    }

                    assignedEngineer = await _db.Engineers.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId.Value);
                    if (assignedEngineer is null)
                        return Error.NotFound("LaborAttendance.EngineerNotFound", "Assigned engineer not found.");

                    toStatusId = s.InProgress;
                    break;
                case "validate":
                    // Assigned (InProgress) → Validated (Completed)
                    if (request.StatusId != s.InProgress)
                    {
                        // No team lead in the department: the request may still be sitting in
                        // New/Pending because there was never anyone to run the "assign" step.
                        // Let a department engineer claim + validate it in one action.
                        if (request.StatusId == s.New && engineer != null
                            && await CanAutoClaimNoTeamLeadRequestAsync(request.DepartmentId, engineer.Id))
                            assignedEngineer = engineer;
                        else
                            return Error.Validation("LaborAttendance.InvalidAction", "Only Assigned requests can be validated.");
                    }
                    toStatusId = s.Completed;
                    break;
                case "reject":
                    // Assigned (InProgress) → Rejected
                    if (request.StatusId != s.InProgress)
                    {
                        // Same no-team-lead direct-action allowance as "validate" above.
                        if (request.StatusId == s.New && engineer != null
                            && await CanAutoClaimNoTeamLeadRequestAsync(request.DepartmentId, engineer.Id))
                            assignedEngineer = engineer;
                        else
                            return Error.Validation("LaborAttendance.InvalidAction", "Only Assigned requests can be rejected.");
                    }
                    toStatusId = s.Rejected;
                    break;
                case "missing_info":
                case "missinginfo":
                    toStatusId = s.MissingInformation;
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

                    try
                    {
                        // Fan-out: notify all engineers in departments with NotifyAfterLaborApprove in this branch.
                        // IgnoreQueryFilters on the branch lookups so a soft-deleted project/department
                        // doesn't silently block branch resolution.
                        Guid? branchId = null;
                        if (request.ProjectId.HasValue)
                            branchId = await _db.Projects
                                .IgnoreQueryFilters()
                                .Where(p => p.Id == request.ProjectId.Value)
                                .Select(p => (Guid?)p.BranchId)
                                .FirstOrDefaultAsync();
                        if (!branchId.HasValue && request.DepartmentId.HasValue)
                            branchId = await _db.Departmentes
                                .IgnoreQueryFilters()
                                .Where(d => d.Id == request.DepartmentId.Value)
                                .Select(d => (Guid?)d.BranchId)
                                .FirstOrDefaultAsync();

                        if (!branchId.HasValue)
                        {
                            _logger.LogWarning(
                                "LaborAttendance {RequestId}: cannot resolve branch for fan-out (ProjectId={ProjectId}, DepartmentId={DeptId}). No department notifications will be sent.",
                                id, request.ProjectId, request.DepartmentId);
                        }
                        else
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
                                    await _notificationService.SendFanOutNotificationAsync(
                                        recipientUserId,
                                        "Labor Attendance Request Ready for Processing",
                                        $"Labor request {request.RequestNumber} has been validated and is ready for processing.",
                                        id, deptId, null, "LaborAttendance");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "LaborAttendance {RequestId}: fan-out notification failed after validate action.",
                            id);
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
                case "missing_info":
                case "missinginfo":
                    // Notify the site engineer who submitted the request (the supervisor).
                    if (supervisorUserId.HasValue)
                        await _notificationService.SendNotificationToUserAsync(
                            supervisorUserId.Value,
                            "Labor Attendance Request - Missing Information",
                            $"Your request {request.RequestNumber} requires additional information. {dto.Comments}",
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

            if (request.AssignedToId is null || request.AssignedToId == Guid.Empty)
                return Error.Validation("LaborAttendance.NotAssigned", "Only an already-assigned request can be reassigned.");

            if (request.DepartmentId.HasValue && actingEngineer != null)
            {
                var departmentId = request.DepartmentId.Value;
                var isManager = await IsEngineerTeamLeadOfDepartmentAsync(actingEngineer.Id, departmentId);
                var isCurrentAssignee = request.AssignedToId == actingEngineer.Id;

                if (!isManager && !isCurrentAssignee)
                    return Error.Unauthorized("LaborAttendance.Unauthorized", "Only the department team lead or the current assignee can reassign this request.");
            }

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
