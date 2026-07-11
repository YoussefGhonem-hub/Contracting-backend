using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
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
    public class FinancialClearanceService : IFinancialClearanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;
        private readonly Contracting.Infrustructure.Inteface.Helper.INotificationService _notificationService;
        private readonly ILogger<FinancialClearanceService> _logger;

        public FinancialClearanceService(
            ApplicationDbContext db,
            Storage.AWS3.Services.IStorageService storageService,
            Contracting.Infrustructure.Inteface.Helper.INotificationService notificationService,
            ILogger<FinancialClearanceService> logger)
        {
            _db = db;
            _storageService = storageService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> CreateAsync(CreateFinancialClearanceDto dto)
        {
            // Date range is required on new clearances, and ToDate must not precede FromDate.
            if (!dto.FromDate.HasValue || !dto.ToDate.HasValue)
                return Error.Validation("FinancialClearance.DateRangeRequired", "Both From Date and To Date are required.");
            if (dto.ToDate.Value.Date < dto.FromDate.Value.Date)
                return Error.Validation("FinancialClearance.InvalidDateRange", "To Date must be on or after From Date.");

            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            var itemsTotal = dto.Items.Sum(i => i.Value);

            var clearance = new FinancialClearance
            {
                ClearanceNumber = await GenerateClearanceNumberAsync(),
                EmployeeName = dto.EmployeeName,
                DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId,
                ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId,
                RequestDate = dto.RequestDate,
                FromDate = dto.FromDate,
                ToDate = dto.ToDate,
                AdvanceAmount = dto.AdvanceAmount,
                SpentAmount = itemsTotal,
                RemainingAmount = dto.AdvanceAmount - itemsTotal,
                Notes = dto.Notes,
                StatusId = s.New,
                RequestedById = engineer?.Id
            };

            foreach (var item in dto.Items)
            {
                clearance.Items.Add(new FinancialClearanceItem
                {
                    Code = item.Code,
                    ItemName = item.ItemName,
                    Description = item.Description,
                    Value = item.Value
                });
            }

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var files = dto.Attachments.ToList();
                var uploaded = await _storageService.UploadFiles(files);
                for (int i = 0; i < (uploaded?.Count ?? 0); i++)
                {
                    var f = uploaded![i];
                    clearance.Attachments.Add(new FinancialClearanceAttachment
                    {
                        Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                        FileSize = f.FileSize, Url = f.Url,
                        AttachmentType = dto.AttachmentTypes != null && i < dto.AttachmentTypes.Count
                            ? dto.AttachmentTypes[i] : null
                    });
                }
            }

            clearance.Activities.Add(new FinancialClearanceActivity
            {
                EngineerId = engineer?.Id,
                ToStatusId = s.New,
                ActionType = "Created"
            });

            await _db.FinancialClearances.AddAsync(clearance);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(clearance.Id);
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> UpdateAsync(UpdateFinancialClearanceDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var clearance = await _db.FinancialClearances
                .Include(c => c.Items)
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");
            // Allow editing in New (Draft) state, and in MissingInformation state so the
            // requester can fix/add the missing details before resubmitting (the "submit"
            // action transitions MissingInformation -> InProgress but never lets the
            // underlying fields be corrected on its own).
            if (clearance.StatusId != s.New && clearance.StatusId != s.MissingInformation)
                return Error.Validation("FinancialClearance.CannotEdit", "Only new (draft) or missing-information clearances can be edited.");

            if (dto.EmployeeName is not null) clearance.EmployeeName = dto.EmployeeName;
            if (dto.DepartmentId.HasValue) clearance.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            if (dto.ProjectId.HasValue) clearance.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            if (dto.RequestDate.HasValue) clearance.RequestDate = dto.RequestDate.Value;

            // Validate the resulting date range against whichever value ends up effective.
            var effectiveFrom = dto.FromDate ?? clearance.FromDate;
            var effectiveTo = dto.ToDate ?? clearance.ToDate;
            if (effectiveFrom.HasValue && effectiveTo.HasValue && effectiveTo.Value.Date < effectiveFrom.Value.Date)
                return Error.Validation("FinancialClearance.InvalidDateRange", "To Date must be on or after From Date.");
            if (dto.FromDate.HasValue) clearance.FromDate = dto.FromDate.Value;
            if (dto.ToDate.HasValue) clearance.ToDate = dto.ToDate.Value;

            if (dto.Notes is not null) clearance.Notes = dto.Notes;

            var advanceAmount = dto.AdvanceAmount ?? clearance.AdvanceAmount;

            if (dto.Items != null)
            {
                _db.FinancialClearanceItems.RemoveRange(clearance.Items);
                clearance.Items.Clear();
                foreach (var item in dto.Items)
                {
                    clearance.Items.Add(new FinancialClearanceItem
                    {
                        Code = item.Code,
                        ItemName = item.ItemName,
                        Description = item.Description,
                        Value = item.Value
                    });
                }
            }

            var spentAmount = clearance.Items.Sum(i => i.Value);
            clearance.AdvanceAmount = advanceAmount;
            clearance.SpentAmount = spentAmount;
            clearance.RemainingAmount = advanceAmount - spentAmount;

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var files = dto.Attachments.ToList();
                var uploaded = await _storageService.UploadFiles(files);
                for (int i = 0; i < (uploaded?.Count ?? 0); i++)
                {
                    var f = uploaded![i];
                    clearance.Attachments.Add(new FinancialClearanceAttachment
                    {
                        FinancialClearanceId = clearance.Id,
                        Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                        FileSize = f.FileSize, Url = f.Url,
                        AttachmentType = dto.AttachmentTypes != null && i < dto.AttachmentTypes.Count
                            ? dto.AttachmentTypes[i] : null
                    });
                }
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(clearance.Id);
        }

        public async Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var clearance = await _db.FinancialClearances.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");
            if (clearance.StatusId != s.New)
                return Error.Validation("FinancialClearance.CannotDelete", "Only new (draft) clearances can be deleted.");

            clearance.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult("Deleted successfully.");
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> GetByIdAsync(Guid id)
        {
            var clearance = await _db.FinancialClearances
                .Include(c => c.Items)
                .Include(c => c.Department)
                .Include(c => c.Project)
                .Include(c => c.RequestedBy)
                .Include(c => c.AssignedTo)
                .Include(c => c.Status)
                .Include(c => c.Attachments)
                .Include(c => c.Activities).ThenInclude(a => a.Engineer)
                .Include(c => c.Activities).ThenInclude(a => a.FromStatus)
                .Include(c => c.Activities).ThenInclude(a => a.ToStatus)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");
            return MapToDto(clearance);
        }

        public async Task<PaginatedList<GetFinancialClearanceDto>> GetAllAsync(FinancialClearanceFilterDto filter)
        {
            var query = _db.FinancialClearances
                .Include(c => c.Items)
                .Include(c => c.Department)
                .Include(c => c.Project)
                .Include(c => c.RequestedBy)
                .Include(c => c.AssignedTo)
                .Include(c => c.Status)
                .Include(c => c.Attachments)
                .Include(c => c.Activities).ThenInclude(a => a.Engineer)
                .Include(c => c.Activities).ThenInclude(a => a.FromStatus)
                .Include(c => c.Activities).ThenInclude(a => a.ToStatus)
                .Where(c => !c.IsDeleted)
                .AsNoTracking();

            if (filter.StatusId.HasValue)
                query = query.Where(c => c.StatusId == filter.StatusId);

            if (filter.ProjectId.HasValue)    query = query.Where(c => c.ProjectId == filter.ProjectId);
            if (filter.DepartmentId.HasValue) query = query.Where(c => c.DepartmentId == filter.DepartmentId);
            if (filter.BranchId.HasValue)     query = query.Where(c => c.Project != null && c.Project.BranchId == filter.BranchId);
            if (filter.RequestedById.HasValue) query = query.Where(c => c.RequestedById == filter.RequestedById);
            if (filter.AssignedToId.HasValue) query = query.Where(c => c.AssignedToId == filter.AssignedToId);
            if (filter.FromDate.HasValue)     query = query.Where(c => c.RequestDate >= filter.FromDate);
            if (filter.ToDate.HasValue)       query = query.Where(c => c.RequestDate <= filter.ToDate);
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(c => c.ClearanceNumber!.Contains(filter.Search) || c.EmployeeName!.Contains(filter.Search));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedList<GetFinancialClearanceDto>(
                items.Select(MapToDto).ToList(),
                totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> TakeActionAsync(Guid id, FinancialClearanceActionDto dto)
        {
            try
            {
                return await TakeActionInternalAsync(id, dto);
            }
            catch (Exception ex)
            {
                return Error.Failure("FinancialClearance.Error", ex.InnerException?.Message ?? ex.Message);
            }
        }

        private async Task<ErrorOr<GetFinancialClearanceDto>> TakeActionInternalAsync(Guid id, FinancialClearanceActionDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var clearance = await _db.FinancialClearances
                .Include(c => c.Attachments)
                .Include(c => c.Activities)
                .Include(c => c.RequestedBy)
                .Include(c => c.AssignedTo)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            var fromStatusId = clearance.StatusId;
            Guid toStatusId;
            Engineer? newlyAssignedEngineer = null;
            var actionLower = dto.ActionType.Trim().ToLower();

            switch (actionLower)
            {
                case "assign":
                    if (clearance.StatusId == s.Completed || clearance.StatusId == s.Rejected)
                        return Error.Validation("FinancialClearance.InvalidAction", "Cannot assign a completed or rejected clearance.");
                    if (!dto.AssignedToId.HasValue || dto.AssignedToId == Guid.Empty)
                        return Error.Validation("FinancialClearance.AssignedToRequired", "AssignedToId is required for assign action.");
                    var assignedEngineer = await _db.Engineers.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId);
                    if (assignedEngineer is null)
                        return Error.NotFound("FinancialClearance.EngineerNotFound", "Assigned engineer not found.");
                    clearance.AssignedToId = dto.AssignedToId;
                    newlyAssignedEngineer = assignedEngineer;
                    toStatusId = s.InProgress;
                    break;

                case "submit":
                    // New → InProgress (first submit) or MissingInformation → InProgress (resubmit after fix)
                    if (clearance.StatusId != s.New && clearance.StatusId != s.MissingInformation)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only new or missing-information clearances can be submitted.");
                    toStatusId = s.InProgress;
                    break;

                case "review":
                    // InProgress stays InProgress, just logs the review activity
                    if (clearance.StatusId != s.InProgress)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only submitted clearances can be reviewed.");
                    toStatusId = s.InProgress;
                    break;

                case "approve":
                    // InProgress → Completed
                    if (clearance.StatusId != s.InProgress)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only submitted clearances can be approved.");
                    toStatusId = s.Completed;
                    break;

                case "close":
                    // Completed → Completed (only allowed after Approve, not after Close)
                    if (clearance.StatusId != s.Completed)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only approved clearances can be closed.");

                    var lastActionType = clearance.Activities
                        .OrderByDescending(a => a.CreatedDate)
                        .FirstOrDefault()?.ActionType;
                    if (lastActionType?.Equals("close", StringComparison.OrdinalIgnoreCase) == true)
                        return Error.Validation("FinancialClearance.AlreadyClosed", "This clearance has already been closed.");

                    if (!clearance.Attachments.Any() && dto.Attachment == null)
                        return Error.Validation("FinancialClearance.MissingAttachments", "At least one attachment is required before closing. Upload a file with this action or attach one beforehand.");

                    toStatusId = s.Completed;
                    break;

                case "reject":
                    // New or InProgress → Rejected
                    if (clearance.StatusId == s.Completed || clearance.StatusId == s.Rejected)
                        return Error.Validation("FinancialClearance.InvalidAction", "Cannot reject a completed or already-rejected clearance.");
                    toStatusId = s.Rejected;
                    break;

                case "missing_info":
                case "missinginfo":
                case "needs_update":
                    toStatusId = s.MissingInformation;
                    break;

                default:
                    return Error.Validation("FinancialClearance.UnknownAction", $"Unknown action: {dto.ActionType}. Valid values: Assign, Submit, Review, Approve, Close, Reject, MissingInfo");
            }

            var now = Contracting.Shared.Common.DateTimeHelper.Now;
            var userId = CurrentUser.Id;

            // Use ExecuteUpdateAsync to avoid EF change-tracking concurrency issues
            await _db.FinancialClearances
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.StatusId, toStatusId)
                    .SetProperty(c => c.AssignedToId, clearance.AssignedToId)
                    .SetProperty(c => c.ModifiedDate, now)
                    .SetProperty(c => c.ModifiedBy, userId));

            var activity = new FinancialClearanceActivity
            {
                FinancialClearanceId = clearance.Id,
                EngineerId = engineer?.Id,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId,
                ActionType = dto.ActionType,
                Comments = dto.Comments,
            };
            activity.MarkAsCreated(userId ?? Guid.Empty);
            _db.FinancialClearanceActivities.Add(activity);

            if (dto.Attachment != null)
            {
                var uploaded = await _storageService.UploadFiles(new List<Microsoft.AspNetCore.Http.IFormFile> { dto.Attachment });
                if (uploaded != null && uploaded.Count > 0)
                {
                    var f = uploaded[0];
                    _db.FinancialClearanceAttachments.Add(new FinancialClearanceAttachment
                    {
                        FinancialClearanceId = clearance.Id,
                        Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                        FileSize = f.FileSize, Url = f.Url,
                        AttachmentType = dto.AttachmentType
                    });
                }
            }

            await _db.SaveChangesAsync();

            await NotifyOnTakeActionAsync(clearance, actionLower, newlyAssignedEngineer);

            return await GetByIdAsync(clearance.Id);
        }

        // Every action here changes something a specific person is waiting on - the assignee,
        // or the original requester - so each must notify that person. Wrapped defensively so a
        // notification failure never rolls back or blocks the action itself, and never disappears
        // without a trace the way the Transfer Request notify path once did.
        private async Task NotifyOnTakeActionAsync(FinancialClearance clearance, string actionLower, Engineer? newlyAssignedEngineer)
        {
            try
            {
                switch (actionLower)
                {
                    case "assign":
                        if (newlyAssignedEngineer is not null)
                            await _notificationService.SendNotificationToUserAsync(
                                newlyAssignedEngineer.ApplicationUserId,
                                "Financial Clearance Assigned",
                                $"Financial Clearance {clearance.ClearanceNumber} has been assigned to you.",
                                clearance.Id);
                        break;

                    case "submit":
                        // Resubmission after Missing Information goes back to whoever is already
                        // assigned; the very first submit (New -> InProgress) has no assignee yet.
                        if (clearance.AssignedTo is not null)
                            await _notificationService.SendNotificationToUserAsync(
                                clearance.AssignedTo.ApplicationUserId,
                                "Financial Clearance Resubmitted",
                                $"Financial Clearance {clearance.ClearanceNumber} has been resubmitted for your review.",
                                clearance.Id);
                        break;

                    case "approve":
                        if (clearance.RequestedBy is not null)
                            await _notificationService.SendNotificationToUserAsync(
                                clearance.RequestedBy.ApplicationUserId,
                                "Financial Clearance Approved",
                                $"Your Financial Clearance {clearance.ClearanceNumber} has been approved.",
                                clearance.Id);

                        // Fan-out: notify all engineers in departments with NotifyAfterFinancialClearanceApprove in this branch.
                        // IgnoreQueryFilters on the branch lookups so a soft-deleted project/department
                        // doesn't silently block branch resolution.
                        Guid? fcBranchId = null;
                        if (clearance.ProjectId.HasValue)
                            fcBranchId = await _db.Projects
                                .IgnoreQueryFilters()
                                .Where(p => p.Id == clearance.ProjectId.Value)
                                .Select(p => (Guid?)p.BranchId)
                                .FirstOrDefaultAsync();
                        if (!fcBranchId.HasValue && clearance.DepartmentId.HasValue)
                            fcBranchId = await _db.Departmentes
                                .IgnoreQueryFilters()
                                .Where(d => d.Id == clearance.DepartmentId.Value)
                                .Select(d => (Guid?)d.BranchId)
                                .FirstOrDefaultAsync();

                        if (!fcBranchId.HasValue)
                        {
                            _logger.LogWarning(
                                "FinancialClearance {ClearanceId}: cannot resolve branch for fan-out (ProjectId={ProjectId}, DepartmentId={DeptId}). No department notifications will be sent.",
                                clearance.Id, clearance.ProjectId, clearance.DepartmentId);
                        }
                        else
                        {
                            var fcNotifyDeptIds = await _db.Departmentes
                                .Where(d => d.BranchId == fcBranchId.Value && d.NotifyAfterFinancialClearanceApprove && !d.IsDeleted)
                                .Select(d => d.Id)
                                .ToListAsync();

                            foreach (var deptId in fcNotifyDeptIds)
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
                                        "Financial Clearance Ready for Processing",
                                        $"Financial Clearance {clearance.ClearanceNumber} has been approved and is ready for processing.",
                                        clearance.Id, deptId, null, "FinancialClearance");
                            }
                        }
                        break;

                    case "close":
                        if (clearance.RequestedBy is not null)
                            await _notificationService.SendNotificationToUserAsync(
                                clearance.RequestedBy.ApplicationUserId,
                                "Financial Clearance Closed",
                                $"Your Financial Clearance {clearance.ClearanceNumber} has been closed.",
                                clearance.Id);
                        break;

                    case "reject":
                        if (clearance.RequestedBy is not null)
                            await _notificationService.SendNotificationToUserAsync(
                                clearance.RequestedBy.ApplicationUserId,
                                "Financial Clearance Rejected",
                                $"Your Financial Clearance {clearance.ClearanceNumber} has been rejected.",
                                clearance.Id);
                        break;

                    case "missing_info":
                    case "missinginfo":
                    case "needs_update":
                        if (clearance.RequestedBy is not null)
                            await _notificationService.SendNotificationToUserAsync(
                                clearance.RequestedBy.ApplicationUserId,
                                "Financial Clearance Needs More Information",
                                $"Financial Clearance {clearance.ClearanceNumber} needs more information before it can proceed.",
                                clearance.Id);
                        break;
                }
            }
            catch (Exception ex)
            {
                // Never let a notification failure surface as an error on the action itself -
                // the status change already succeeded and was saved above. But it must be
                // logged, not silently swallowed, or a broken notification path is undiagnosable.
                _logger.LogError(ex,
                    "FinancialClearance {ClearanceId} action {Action}: failed to send notification.",
                    clearance.Id, actionLower);
            }
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> ReassignAsync(Guid id, ReassignFinancialClearanceDto dto)
        {
            try
            {
                return await ReassignInternalAsync(id, dto);
            }
            catch (Exception ex)
            {
                return Error.Failure("FinancialClearance.Error", ex.InnerException?.Message ?? ex.Message);
            }
        }

        private async Task<ErrorOr<GetFinancialClearanceDto>> ReassignInternalAsync(Guid id, ReassignFinancialClearanceDto dto)
        {
            var clearance = await _db.FinancialClearances
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (clearance is null)
                return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");

            if (string.IsNullOrEmpty(CurrentUser.UserId) || !Guid.TryParse(CurrentUser.UserId, out var currentUserId))
                return Error.Unauthorized("Auth.Unauthorized", "User is not authenticated.");

            if (dto.AssignedToId == Guid.Empty)
                return Error.Validation("FinancialClearance.AssignedToRequired", "AssignedToId is required.");

            var newAssignee = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId);
            if (newAssignee is null)
                return Error.NotFound("FinancialClearance.EngineerNotFound", "Assigned engineer not found.");

            if (clearance.AssignedToId == dto.AssignedToId)
                return Error.Validation("FinancialClearance.AlreadyAssigned", "The request is already assigned to this engineer.");

            if (clearance.StatusId is null)
                return Error.Validation("FinancialClearance.InvalidState", "Cannot reassign a clearance with no status.");

            var actingEngineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId);

            var userId = CurrentUser.Id;
            var now = DateTimeHelper.Now;

            // Update the assignee directly to avoid EF tracking issues with unloaded collections.
            await _db.FinancialClearances
                .Where(c => c.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(c => c.AssignedToId, dto.AssignedToId)
                    .SetProperty(c => c.ModifiedDate, now)
                    .SetProperty(c => c.ModifiedBy, userId));

            var activity = new FinancialClearanceActivity
            {
                FinancialClearanceId = clearance.Id,
                EngineerId = actingEngineer?.Id,
                FromStatusId = clearance.StatusId,
                ToStatusId = clearance.StatusId.Value,
                ActionType = "reassign",
                Comments = dto.Comments
            };
            activity.MarkAsCreated(userId ?? Guid.Empty);
            _db.FinancialClearanceActivities.Add(activity);
            await _db.SaveChangesAsync();

            try
            {
                await _notificationService.SendNotificationToUserAsync(
                    newAssignee.ApplicationUserId,
                    "Financial Clearance Request Reassigned",
                    $"Request {clearance.ClearanceNumber} has been reassigned to you.",
                    id);
            }
            catch (Exception ex)
            {
                // The reassignment itself already succeeded and was saved above - a notification
                // failure must not turn a successful reassign into an API error response.
                _logger.LogError(ex, "FinancialClearance {ClearanceId} reassign: failed to send notification.", id);
            }

            return await GetByIdAsync(clearance.Id);
        }

        private async Task<string> GenerateClearanceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _db.FinancialClearances.CountAsync(c => c.CreatedDate.Year == year);
            return $"FIN-{year}-{(count + 1):D5}";
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

        private GetFinancialClearanceDto MapToDto(FinancialClearance c) => new()
        {
            Id = c.Id,
            ClearanceNumber = c.ClearanceNumber,
            EmployeeName = c.EmployeeName,
            DepartmentId = c.DepartmentId,
            Department = c.Department is null ? null : new GetDepartmentDto { Id = c.Department.Id, nameEn = c.Department.nameEn, nameAr = c.Department.nameAr },
            ProjectId = c.ProjectId,
            Project = c.Project is null ? null : new GetProjectDto { Id = c.Project.Id, nameEn = c.Project.nameEn, nameAr = c.Project.nameAr },
            RequestDate = c.RequestDate,
            FromDate = c.FromDate,
            ToDate = c.ToDate,
            AdvanceAmount = c.AdvanceAmount,
            Total = c.SpentAmount,
            RemainingAmount = c.RemainingAmount,
            Notes = c.Notes,
            Items = c.Items.Select(i => new GetFinancialClearanceItemDto
            {
                Id = i.Id,
                Code = i.Code,
                ItemName = i.ItemName,
                Description = i.Description,
                Value = i.Value
            }).ToList(),
            StatusId = c.StatusId,
            Status = MapStatus(c.Status),
            RequestedById = c.RequestedById,
            RequestedBy = c.RequestedBy is null ? null : new GetEngineerDto { Id = c.RequestedBy.Id, nameEn = c.RequestedBy.nameEn, nameAr = c.RequestedBy.nameAr },
            AssignedToId = c.AssignedToId,
            AssignedTo = c.AssignedTo is null ? null : new GetEngineerDto { Id = c.AssignedTo.Id, nameEn = c.AssignedTo.nameEn, nameAr = c.AssignedTo.nameAr },
            CreatedDate = c.CreatedDate,
            Attachments = c.Attachments.Select(a => new GetFinancialClearanceAttachmentDto
            {
                Id = a.Id, Key = a.Key, FileName = a.FileName,
                Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url,
                AttachmentType = a.AttachmentType
            }).ToList(),
            Activities = c.Activities.Select(a => new GetFinancialClearanceActivityDto
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
            }).ToList()
        };
    }
}
