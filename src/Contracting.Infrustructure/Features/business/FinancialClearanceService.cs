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

namespace Contracting.Infrustructure.Features.business
{
    public class FinancialClearanceService : IFinancialClearanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;
        private readonly Contracting.Infrustructure.Inteface.Helper.INotificationService _notificationService;

        public FinancialClearanceService(
            ApplicationDbContext db,
            Storage.AWS3.Services.IStorageService storageService,
            Contracting.Infrustructure.Inteface.Helper.INotificationService notificationService)
        {
            _db = db;
            _storageService = storageService;
            _notificationService = notificationService;
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> CreateAsync(CreateFinancialClearanceDto dto)
        {
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
                    ItemName = item.ItemName,
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
            // Only allow editing when in New state (Draft)
            if (clearance.StatusId != s.New)
                return Error.Validation("FinancialClearance.CannotEdit", "Only new (draft) clearances can be edited.");

            if (dto.EmployeeName is not null) clearance.EmployeeName = dto.EmployeeName;
            if (dto.DepartmentId.HasValue) clearance.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            if (dto.ProjectId.HasValue) clearance.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            if (dto.RequestDate.HasValue) clearance.RequestDate = dto.RequestDate.Value;
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
                        ItemName = item.ItemName,
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
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            var fromStatusId = clearance.StatusId;
            Guid toStatusId;

            switch (dto.ActionType.Trim().ToLower())
            {
                case "assign":
                    if (clearance.StatusId != s.New)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only new clearances can be assigned.");
                    if (!dto.AssignedToId.HasValue || dto.AssignedToId == Guid.Empty)
                        return Error.Validation("FinancialClearance.AssignedToRequired", "AssignedToId is required for assign action.");
                    var assignedEngineer = await _db.Engineers.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId);
                    if (assignedEngineer is null)
                        return Error.NotFound("FinancialClearance.EngineerNotFound", "Assigned engineer not found.");
                    clearance.AssignedToId = dto.AssignedToId;
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

                    if (!clearance.Attachments.Any())
                        return Error.Validation("FinancialClearance.MissingAttachments", "Attachments are required before closing.");

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
                    // InProgress → MissingInformation (office engineer requests more info from submitter)
                    if (clearance.StatusId != s.InProgress)
                        return Error.Validation("FinancialClearance.InvalidAction", "Missing info can only be requested on a submitted clearance.");
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
            await _db.SaveChangesAsync();

            return await GetByIdAsync(clearance.Id);
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

            await _notificationService.SendNotificationToUserAsync(
                newAssignee.ApplicationUserId,
                "Financial Clearance Request Reassigned",
                $"Request {clearance.ClearanceNumber} has been reassigned to you.",
                id);

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
            AdvanceAmount = c.AdvanceAmount,
            Total = c.SpentAmount,
            RemainingAmount = c.RemainingAmount,
            Notes = c.Notes,
            Items = c.Items.Select(i => new GetFinancialClearanceItemDto
            {
                Id = i.Id,
                ItemName = i.ItemName,
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
