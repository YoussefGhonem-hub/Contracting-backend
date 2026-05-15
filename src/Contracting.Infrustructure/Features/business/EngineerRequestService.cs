using Contracting.Shared.Resources;
using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using ErrorOr;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Storage.AWS3.Services;
using Storage.AWS3.Models;
namespace Contracting.Infrustructure.Features.business;



public class EngineerRequestService : IEngineerRequestService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IStorageService _storageService;
    private const string AutomaticStatusActionType = "StatusChangedAuto";
    private static readonly string[] InProgressKeywords = { "in progress", "progress", "processing", "working" };
    private static readonly string[] DelayedKeywords = { "delay", "delayed", "late", "overdue" };
    private static readonly string[] CompletedKeywords = { "completed", "complete", "done", "finished", "finish", "closed" };
    private static readonly string[] NewPendingKeywords = { "new", "pending", "open", "submitted", "created" };
    private static readonly string[] RejectedKeywords = { "rejected", "reject", "denied", "deny" };
    private static readonly string[] PendingInfoKeywords = { "missing information", "missing info", "missing_information" };

    private static bool StatusMatchesKeywords(Contracting.Domain.Entities.master.Status status, string[] keywords)
        => keywords.Any(k =>
            (status.Code?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameEn?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameAr?.Contains(k, StringComparison.OrdinalIgnoreCase) == true));


    public EngineerRequestService(ApplicationDbContext db, IMapper mapper, INotificationService notificationService, IStringLocalizer<SharedResources> localizer, IStorageService storageService)
    {
        _db = db;
        _mapper = mapper;
        _notificationService = notificationService;
        _localizer = localizer;
        _storageService = storageService;
    }

    // ---------------- CREATE ----------------
    public async Task<GetAllEngineerRequestDto> CreateEngineerRequestAsync(CreateEngineerRequestDto dto)
    {
        var roles = CurrentUser.Roles;
        var isSiteEngineer = roles.Any(r => r.Equals(RoleNames.Siteengineer, StringComparison.OrdinalIgnoreCase));
        if (!isSiteEngineer)
            return null!;

        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        var request = _mapper.Map<EngineerRequest>(dto);
        request.EngineerId = engineer?.Id;

        var firstStatus = await _db.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.orderNumber == 1);

        request.StatusId = firstStatus?.Id ?? Guid.Empty;

        // Handle empty GUIDs
        if (request.ProjectId == Guid.Empty) request.ProjectId = null;
        if (request.DepartmentId == Guid.Empty) request.DepartmentId = null;
        if (request.PriorityId == Guid.Empty) request.PriorityId = null;
        if (request.EngineerId == Guid.Empty) request.EngineerId = null;

        // Find the Team Lead for the selected department (via EngineerDepartments first, then legacy)
        var teamLead = await _db.EngineerDepartments
            .Where(ed => ed.DepartmentId == request.DepartmentId && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
            .Select(ed => ed.Engineer!.ApplicationUserId)
            .FirstOrDefaultAsync();

        if (teamLead == Guid.Empty)
        {
            teamLead = await (from eng in _db.Engineers
                          join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                          join role in _db.Roles on userRole.RoleId equals role.Id
                          where eng.DepartmentId == request.DepartmentId && role.Name == RoleNames.Teamleadengineer
                          select eng.ApplicationUserId)
                  .FirstOrDefaultAsync();
        }

        // Handle notes (and their attachments)
        if (dto.EngineerRequestNotes != null && dto.EngineerRequestNotes.Any())
        {
            request.EngineerRequestNotes = new List<EngineerRequestNotes>();
            foreach (var noteDto in dto.EngineerRequestNotes)
            {
                var note = _mapper.Map<EngineerRequestNotes>(noteDto);
                note.EngineerId = engineer?.Id;
                note.EngineerRequest = request;

                // Handle note attachments
                if (noteDto.Attachments != null && noteDto.Attachments.Any())
                {
                    var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                    note.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
                    {
                        Key = f.Key,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        FileSize = f.FileSize,
                        Url = f.Url
                    }).ToList();
                }

                request.EngineerRequestNotes.Add(note);
            }
        }

        // Handle request-level attachments
        if (dto.Attachments != null && dto.Attachments.Any())
        {
            var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
            request.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
            {
                Key = f.Key,
                FileName = f.FileName,
                Extension = f.Extension,
                FileSize = f.FileSize,
                Url = f.Url
            }).ToList();
        }

        await _db.EngineerRequests.AddAsync(request);
        
        // Save special field values
        if (dto.SpecialFieldValues != null && dto.SpecialFieldValues.Any())
        {
            foreach (var sfv in dto.SpecialFieldValues)
            {
                var specialFieldValue = new EngineerRequestSpecialFieldValue
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = sfv.DepartmentSpecialFieldId,
                    value = sfv.value
                };
                await _db.EngineerRequestSpecialFieldValues.AddAsync(specialFieldValue);
            }
        }

        // Create initial activity for request creation
        var createActivity = new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = engineer?.Id,
            StatusId = request.StatusId,
            ActionType = "Created"
        };
        await _db.EngineerRequestActivites.AddAsync(createActivity);
        
        await _db.SaveChangesAsync();

        // Reload with navigation properties (including attachments)
        var createdRequest = await _db.EngineerRequests
                            .Include(r => r.Project)
                            .Include(r => r.Department)
                            .Include(r => r.Priority)
                            .Include(r => r.Status)
                            .Include(r => r.Engineer)
                                .ThenInclude(e => e.Department)
                            .Include(r => r.EngineerRequestNotes)
                                .ThenInclude(n => n.EngineerRequestAttachments)
                            .Include(r => r.EngineerRequestAttachments)
                            .Include(r=>r.EngineerRequestActivites)
                            .Include(r => r.SpecialFieldValues)
                                .ThenInclude(v => v.DepartmentSpecialField)
                                    .ThenInclude(psf => psf.SpecialField)
                            .AsSplitQuery()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(r => r.Id == request.Id);


        if (teamLead == Guid.Empty)
        {
            // No team lead found, notify all engineers in the department (via EngineerDepartments + legacy)
            var departmentEngineerIds = await _db.EngineerDepartments
                .Where(ed => ed.DepartmentId == request.DepartmentId)
                .Select(ed => ed.Engineer!.ApplicationUserId)
                .ToListAsync();

            var legacyEngineerIds = await _db.Engineers
                .Where(e => e.DepartmentId == request.DepartmentId)
                .Select(e => e.ApplicationUserId)
                .ToListAsync();

            var allEngineerUserIds = departmentEngineerIds.Union(legacyEngineerIds).Distinct();
            foreach (var engineerUserId in allEngineerUserIds)
            {
                await _notificationService.SendNotificationToUserAsync(
                    engineerUserId,
                    _localizer[SharedResourcesKeys.NotificationNewRequestTitle],
                    _localizer[SharedResourcesKeys.NotificationNewRequestBody],
                    request.Id,
                    request.DepartmentId);
            }
        }
        else
        {
            // Notify only the team lead
            await _notificationService.SendNotificationToUserAsync(
                teamLead,
                _localizer[SharedResourcesKeys.NotificationNewRequestTitle],
                _localizer[SharedResourcesKeys.NotificationNewRequestBody],
                request.Id,
                request.DepartmentId);
        }

        return _mapper.Map<GetAllEngineerRequestDto>(createdRequest);
    }

    // ---------------- UPDATE ----------------
    public async Task<ErrorOr<GetAllEngineerRequestDto>> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto)
    {
        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        var request = await _db.EngineerRequests
            .Include(r => r.EngineerRequestNotes)
            .Include(r => r.SpecialFieldValues)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == dto.Id);
        if (request is null)
            return Error.NotFound("Request.NotFound", _localizer[SharedResourcesKeys.RequestNotFound]);

        // Rejected requests are final — cannot be edited
        if (request.Status != null && StatusMatchesKeywords(request.Status, RejectedKeywords))
            return Error.Forbidden("Request.Rejected", _localizer[SharedResourcesKeys.RequestRejectedCannotEdit]);

        bool isPendingInfo = request.Status != null && StatusMatchesKeywords(request.Status, PendingInfoKeywords);

        // Allow update only when: status is Missing Information, OR request not yet assigned
        if (!isPendingInfo && request.assignToId != null && request.assignToId != Guid.Empty)
            return Error.Forbidden("Request.AlreadyActioned", _localizer[SharedResourcesKeys.RequestAlreadyActioned]);

        // When in Missing Information status, at least one note is mandatory (attachment is optional)
        if (isPendingInfo)
        {
            bool hasNote = dto.EngineerRequestNotes != null && dto.EngineerRequestNotes.Any();
            if (!hasNote)
                return Error.Validation("Request.MissingNote", _localizer[SharedResourcesKeys.MissingInfoRequiresNoteAndAttachment]);
        }

        // Update fields
        request.ProjectId = dto.ProjectId == Guid.Empty ? request.ProjectId : dto.ProjectId;
        request.DepartmentId = dto.DepartmentId == Guid.Empty ? request.DepartmentId : dto.DepartmentId;
        request.PriorityId = dto.PriorityId == Guid.Empty ? request.PriorityId : dto.PriorityId;
        request.RequestTitle = dto.RequestTitle ?? request.RequestTitle;
        request.Descreption = dto.Descreption;

        // Handle notes
        if (dto.EngineerRequestNotes != null)
        {
            // Remove notes not in DTO
            var dtoNoteIds = dto.EngineerRequestNotes
                .Where(n => n.Id.HasValue)
                .Select(n => n.Id.Value)
                .ToList();

            var notesToRemove = request.EngineerRequestNotes
                .Where(n => !dtoNoteIds.Contains(n.Id))
                .ToList();

            _db.EngineerRequestNotes.RemoveRange(notesToRemove);

            // Update or add notes
            foreach (var noteDto in dto.EngineerRequestNotes)
            {
                if (noteDto.Id.HasValue)
                {
                    // Update existing note
                    var existingNote = request.EngineerRequestNotes.FirstOrDefault(n => n.Id == noteDto.Id);
                    if (existingNote != null)
                    {
                        existingNote.note = noteDto.note;
                        // If there are new attachments for existing note, upload and add
                        if (noteDto.Attachments != null && noteDto.Attachments.Any())
                        {
                            var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                            if (uploaded != null && uploaded.Any())
                            {
                                existingNote.EngineerRequestAttachments ??= new List<EngineerRequestAttachment>();
                                existingNote.EngineerRequestAttachments = existingNote.EngineerRequestAttachments.Concat(uploaded.Select(f => new EngineerRequestAttachment
                                {
                                    Key = f.Key,
                                    FileName = f.FileName,
                                    Extension = f.Extension,
                                    FileSize = f.FileSize,
                                    Url = f.Url
                                })).ToList();
                            }
                        }
                    }
                }
                else
                {
                    // Add new note
                    var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                    newNote.EngineerRequestId = request.Id;
                    newNote.EngineerId = engineer?.Id;
                    // Handle attachments for new note
                    if (noteDto.Attachments != null && noteDto.Attachments.Any())
                    {
                        var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                        newNote.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
                        {
                            Key = f.Key,
                            FileName = f.FileName,
                            Extension = f.Extension,
                            FileSize = f.FileSize,
                            Url = f.Url
                        }).ToList();
                    }

                    await _db.EngineerRequestNotes.AddAsync(newNote);
                }
            }
        }

        // Handle request-level new attachments
        if (dto.Attachments != null && dto.Attachments.Any())
        {
            var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
            if (uploaded != null && uploaded.Any())
            {
                request.EngineerRequestAttachments ??= new List<EngineerRequestAttachment>();
                request.EngineerRequestAttachments = request.EngineerRequestAttachments.Concat(uploaded.Select(f => new EngineerRequestAttachment
                {
                    Key = f.Key,
                    FileName = f.FileName,
                    Extension = f.Extension,
                    FileSize = f.FileSize,
                    Url = f.Url,
                    EngineerRequestId = request.Id
                })).ToList();
            }
        }

        // Handle special field values
        if (dto.SpecialFieldValues != null)
        {
            // Remove existing special field values
            if (request.SpecialFieldValues != null && request.SpecialFieldValues.Any())
            {
                _db.EngineerRequestSpecialFieldValues.RemoveRange(request.SpecialFieldValues);
            }

            // Add new special field values
            foreach (var sfv in dto.SpecialFieldValues)
            {
                var specialFieldValue = new EngineerRequestSpecialFieldValue
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = sfv.DepartmentSpecialFieldId,
                    value = sfv.value
                };
                await _db.EngineerRequestSpecialFieldValues.AddAsync(specialFieldValue);
            }
        }

        await _db.SaveChangesAsync();

        // If request was in Missing Information, auto-reset status back to New
        if (isPendingInfo)
        {
            var allStatuses = await _db.Statuses
                .AsNoTracking()
                .OrderBy(s => s.orderNumber)
                .ToListAsync();

            var targetStatus = allStatuses.FirstOrDefault(s => StatusMatchesKeywords(s, NewPendingKeywords))
                            ?? allStatuses.OrderBy(s => s.orderNumber).FirstOrDefault();

            if (targetStatus != null && targetStatus.Id != request.StatusId)
            {
                request.StatusId = targetStatus.Id;
                var resetActivity = new EngineerRequestActivite
                {
                    EngineerRequestId = request.Id,
                    EngineerId = engineer?.Id,
                    StatusId = targetStatus.Id,
                    ActionType = AutomaticStatusActionType
                };
                await _db.EngineerRequestActivites.AddAsync(resetActivity);
                await _db.SaveChangesAsync();
            }
        }

        // Reload with navigation properties (including attachments)
        var updatedRequest = await _db.EngineerRequests
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Priority)
            .Include(r => r.Engineer)
                .ThenInclude(e => e.Department)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestAttachments)
            .Include(r => r.SpecialFieldValues)
                .ThenInclude(v => v.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        return _mapper.Map<GetAllEngineerRequestDto>(updatedRequest);
    }

    // ---------------- DELETE ----------------
    public async Task<GenericResponse> DeleteEngineerRequestAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests.FindAsync(requestId);
        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);

        // Check if action has been taken
        if (request.assignToId != Guid.Empty)
        {
            // Request has been actioned, cannot delete
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestAlreadyActioned]);
        }

        _db.EngineerRequests.Remove(request);
        await _db.SaveChangesAsync();
        return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.RequestDeleteSuccess]);
    }

    // ---------------- GET ALL BY DEPARTMENT (FOR MANAGERS) ----------------
    public async Task<PaginatedList<GetAllEngineerRequestDto>> GetAllEngineerRequestsByDepartmentAsync(
        Guid departmentId,
        BaseFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r=>r.Status)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.SpecialFieldValues)
                    .ThenInclude(v => v.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .Where(r => r.DepartmentId == departmentId)
                .AsSplitQuery()
                .AsNoTracking();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var requestDtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);

            return new PaginatedList<GetAllEngineerRequestDto>(
                requestDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    // ---------------- FILTER ENGINEER REQUESTS ----------------
    public async Task<PaginatedList<GetAllEngineerRequestDto>> FilterEngineerRequestsAsync(
        EngineerRequestFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.EngineerRequestAttachments)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.Department)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.SpecialFieldValues)
                    .ThenInclude(v => v.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == filter.DepartmentId.Value);
            }

            if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
            {
                query = query.Where(r => r.StatusId == filter.StatusId.Value);
            }

            if (filter.EngineerId.HasValue && filter.EngineerId.Value != Guid.Empty)
            {
                query = query.Where(r => r.assignToId == filter.EngineerId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.startDate.HasValue && r.startDate.Value >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.endDate.HasValue && r.endDate.Value <= filter.ToDate.Value);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var requestDtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);

            return new PaginatedList<GetAllEngineerRequestDto>(
                requestDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    // ---------------- GET BY ID ----------------
    public async Task<GetAllEngineerRequestDto> GetEngineerRequestByIdAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.Engineer)
                .ThenInclude(e => e.Department)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Engineer)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Status)
            .Include(r => r.SpecialFieldValues)
                .ThenInclude(v => v.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requestId);

        return request is null ? null! : _mapper.Map<GetAllEngineerRequestDto>(request);
    }

    // ---------------- CHECK IF ENGINEER IS MANAGER ----------------
    public async Task<bool> IsEngineerManagerOfDepartmentAsync(Guid engineerId, Guid departmentId)
    {
        var teamleadRoleName = RoleNames.Teamleadengineer;

        // Check via EngineerDepartments (multi-department role assignment)
        var isTeamLeadViaDeptRole = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == engineerId
                         && ed.DepartmentId == departmentId
                         && ed.Role != null && ed.Role.Name == teamleadRoleName);

        if (isTeamLeadViaDeptRole) return true;

        // Fallback: check via legacy DepartmentId + global UserRoles
        var isTeamLead = await (from eng in _db.Engineers
                                join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                join role in _db.Roles on userRole.RoleId equals role.Id
                                where eng.Id == engineerId && eng.DepartmentId == departmentId && role.Name == teamleadRoleName
                                select eng.Id)
                    .AnyAsync();

        return isTeamLead;
    }

    // ---------------- TAKE ACTION ON REQUEST ----------------
    public async Task<GenericResponse> TakeActionOnRequestAsync(Guid requestId, Guid currentUserId, TakeActionRequestDto actionDto)
    {

        var request = await _db.EngineerRequests
            .Include(r => r.EngineerRequestNotes)
            .Include(x=>x.Engineer).ThenInclude(x=>x.ApplicationUser)
            .Include(r => r.assignTo)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == requestId);


        var currentEngineerId = await _db.Engineers
            .Where(e => e.ApplicationUserId == currentUserId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);

        // Rejected requests are final — no further actions allowed
        if (request.Status != null && StatusMatchesKeywords(request.Status, RejectedKeywords))
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestRejectedCannotAction]);
      
        // Get department manager
        var departmentId = request.DepartmentId;
        if (!departmentId.HasValue)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNoDepartment]);

        // Check if department has a team lead
        bool departmentHasTeamLead = await DepartmentHasTeamLeadAsync(departmentId.Value);

        // Check if current user is a team lead for this request's department (via EngineerDepartments or legacy)
        var isManager = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == currentEngineerId
                         && ed.DepartmentId == departmentId.Value
                         && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer);
        if (!isManager)
        {
            isManager = await (from eng in _db.Engineers
                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                               join role in _db.Roles on userRole.RoleId equals role.Id
                               where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                               select eng.Id)
                       .AnyAsync();
        }

        var isAssigned = request.assignToId.HasValue && request.assignToId.Value == currentEngineerId;

        // If assignToId is set
        if (request.assignToId.HasValue && request.assignToId.Value != Guid.Empty)
        {
            if (departmentHasTeamLead)
            {
                // If department has a team lead, only assigned user or team lead can take action
                if (!isManager && !isAssigned)
                    return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
            }
            else
            {
                // No team lead: only assigned user can take action
                if (!isAssigned)
                    return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
            }
        }
        else if (departmentHasTeamLead)
        {
            // If department has a team lead, only team lead can take action
            if (!isManager)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }
        else
        {
            // If no team lead and no assignment, allow any engineer in the department to take action
            var isEngineerInDepartment = await _db.EngineerDepartments.AnyAsync(ed => ed.EngineerId == currentEngineerId && ed.DepartmentId == departmentId.Value)
                || await _db.Engineers.AnyAsync(e => e.ApplicationUserId == currentUserId && e.DepartmentId == departmentId.Value);
            if (!isEngineerInDepartment)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }

        // Track previous status and assignment for activity
        var previousStatusId = request.StatusId;
        var previousAssignedId = request.assignToId;
        bool assignmentChanged = false;

        // If manager, allow assignment
        if (isManager && actionDto.assignToId.HasValue)
        {
            request.assignToId = actionDto.assignToId.Value;
            
            // Create activity for assignment
            if (previousAssignedId != actionDto.assignToId.Value)
            {
                assignmentChanged = true;
                var assignActivity = new EngineerRequestActivite
                {
                    EngineerRequestId = request.Id,
                    EngineerId = currentEngineerId,
                    StatusId = request.StatusId,
                    ActionType = "Assigned"
                };
                await _db.EngineerRequestActivites.AddAsync(assignActivity);
            }
        }
        // If assigned engineer, do not allow assignment change
        else if (isAssigned && actionDto.assignToId.HasValue && actionDto.assignToId.Value != currentEngineerId)
        {
            // Assigned engineer cannot reassign
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);
        }
        // Department without team lead: auto-assign the request to the engineer taking action
        else if (!departmentHasTeamLead && !isAssigned
            && (!request.assignToId.HasValue || request.assignToId.Value == Guid.Empty))
        {
            request.assignToId = currentEngineerId;
            assignmentChanged = true;
            var assignActivity = new EngineerRequestActivite
            {
                EngineerRequestId = request.Id,
                EngineerId = currentEngineerId,
                StatusId = request.StatusId,
                ActionType = "Assigned"
            };
            await _db.EngineerRequestActivites.AddAsync(assignActivity);
        }

        // Update status, note, and note date
        if (actionDto.statusId.HasValue && actionDto.statusId.Value != previousStatusId)
        {
            request.StatusId = actionDto.statusId.Value;
            
            // Create activity for status change
            var statusActivity = new EngineerRequestActivite
            {
                EngineerRequestId = request.Id,
                EngineerId = currentEngineerId,
                StatusId = actionDto.statusId.Value,
                ActionType = "StatusChanged"
            };
            await _db.EngineerRequestActivites.AddAsync(statusActivity);
        }

        //if (isAssigned && request.timeDuration != actionDto.timeDuration.Value)
        //{
        //    return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.TimeDurationMismatch]);
        //}

        if (actionDto.timeDuration.HasValue && actionDto.timeDuration != 0)
        {
            request.timeDuration = actionDto.timeDuration.Value;
            request.startDate = actionDto.startDate.Value;

            // Block endDate change if delivery date has already been confirmed
            if (request.IsDeliveryDateConfirmed && actionDto.endDate != request.endDate)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.DeliveryDateAlreadyConfirmed]);

            request.endDate = actionDto.endDate;
        }                        

        if (actionDto.EngineerRequestNotes != null && actionDto.EngineerRequestNotes.Any())
        {
            foreach (var noteDto in actionDto.EngineerRequestNotes)
            {
                var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                newNote.EngineerRequestId = request.Id;
                newNote.EngineerId = currentEngineerId;

                // Handle attachments for the new note
                if (noteDto.Attachments != null && noteDto.Attachments.Any())
                {
                    var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                    newNote.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
                    {
                        Key = f.Key,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        FileSize = f.FileSize,
                        Url = f.Url
                    }).ToList();
                }

                await _db.EngineerRequestNotes.AddAsync(newNote);
            }
        }

        await _db.SaveChangesAsync();

        // Send notifications
        if (isManager && request.EngineerId.HasValue && request.Engineer?.ApplicationUserId != Guid.Empty)
        {
            // Notify the request creator that team lead took action
            await _notificationService.SendNotificationToUserAsync(
                request.Engineer.ApplicationUserId,
                _localizer[SharedResourcesKeys.NotificationRequestUpdatedTitle],
                _localizer[SharedResourcesKeys.NotificationRequestUpdatedBody],
                request.Id);
        }

        // Only notify when assignment actually changed (new assignment or reassignment)
        if (assignmentChanged && request.assignToId.HasValue)
        {
            var assignEngineer = await _db.Engineers
            .FirstOrDefaultAsync(r => r.Id == request.assignToId);
            // Notify the engineer who was assigned
            await _notificationService.SendNotificationToUserAsync(
                assignEngineer.ApplicationUserId,
                _localizer[SharedResourcesKeys.NotificationAssignedTitle],
                _localizer[SharedResourcesKeys.NotificationAssignedBody],
                request.Id);
        }

        // If the current user is the assigned engineer and not the team lead
        if (isAssigned && request.EngineerId.HasValue)
        {
            var creator = await _db.Engineers
                .Where(e => e.Id == request.EngineerId.Value)
                .Select(e => e.ApplicationUserId)
                .FirstOrDefaultAsync();

            // Notify the request creator that assigned engineer took action
            await _notificationService.SendNotificationToUserAsync(
                creator,
                _localizer[SharedResourcesKeys.NotificationAssignedUpdateTitle],
                _localizer[SharedResourcesKeys.NotificationAssignedUpdateBody],
                request.Id);
        }

        // Notify request creator if status was changed to Missing Information
        if (actionDto.statusId.HasValue)
        {
            var newStatus = await _db.Statuses.FindAsync(request.StatusId);
            if (newStatus != null && StatusMatchesKeywords(newStatus, PendingInfoKeywords)
                && request.EngineerId.HasValue)
            {
                var creatorAppUserId = request.Engineer?.ApplicationUserId ?? Guid.Empty;
                if (creatorAppUserId != Guid.Empty)
                {
                    await _notificationService.SendNotificationToUserAsync(
                        creatorAppUserId,
                        _localizer[SharedResourcesKeys.NotificationMissingInfoTitle],
                        _localizer[SharedResourcesKeys.NotificationMissingInfoBody],
                        request.Id);
                }
            }
        }

        return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.ActionTakenSuccess]);
    }

    // ---------------- REASSIGN REQUEST ----------------
    public async Task<GenericResponse> ReassignEngineerRequestAsync(Guid requestId, Guid currentUserId, ReassignEngineerRequestDto dto)
    {
        if (dto == null || dto.assignToId == Guid.Empty)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);

        var request = await _db.EngineerRequests
            .Include(r => r.Engineer)
            .Include(r => r.assignTo)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);

        if (!request.assignToId.HasValue || request.assignToId.Value == Guid.Empty)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);

        var departmentId = request.DepartmentId;
        if (!departmentId.HasValue)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNoDepartment]);

        var departmentHasTeamLead = await DepartmentHasTeamLeadAsync(departmentId.Value);

        var currentEngineerId = await _db.Engineers
            .Where(e => e.ApplicationUserId == currentUserId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        // Check if current user is a team lead for this request's department (via EngineerDepartments or legacy)
        var isManager = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == currentEngineerId
                         && ed.DepartmentId == departmentId.Value
                         && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer);
        if (!isManager)
        {
            isManager = await (from eng in _db.Engineers
                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                               join role in _db.Roles on userRole.RoleId equals role.Id
                               where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                               select eng.Id)
                       .AnyAsync();
        }

        var isAssigned = request.assignToId.Value == currentEngineerId;

        if (departmentHasTeamLead)
        {
            if (!isManager)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }
        else
        {
            if (!isAssigned)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }

        if (request.assignToId.Value == dto.assignToId)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);

        request.assignToId = dto.assignToId;

        var reassignActivity = new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = currentEngineerId,
            StatusId = request.StatusId,
            ActionType = "Reassigned"
        };
        await _db.EngineerRequestActivites.AddAsync(reassignActivity);

        await _db.SaveChangesAsync();

        var assignEngineer = await _db.Engineers
            .FirstOrDefaultAsync(r => r.Id == request.assignToId);

        if (assignEngineer?.ApplicationUserId != null && assignEngineer.ApplicationUserId != Guid.Empty)
        {
            await _notificationService.SendNotificationToUserAsync(
                assignEngineer.ApplicationUserId,
                _localizer[SharedResourcesKeys.NotificationReassignedTitle],
                _localizer[SharedResourcesKeys.NotificationReassignedBody],
                request.Id);
        }

        return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.ActionTakenSuccess]);
    }



    public async Task<PaginatedList<GetAllEngineerRequestDto>> GetCreatedRequestOrapplaied(EngineerRequestParticipationFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            var roles = CurrentUser.Roles;
            var isAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                      || r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

            var engineer = isAdmin ? null : await _db.Engineers
                .Include(x => x.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

            if (!isAdmin && engineer == null)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.EngineerRequestAttachments)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.Department)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.SpecialFieldValues)
                    .ThenInclude(v => v.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ProjectId == filter.ProjectId.Value);
            }

            // AssignToId filter: only apply for admins as a data filter.
            // For non-admins, visibility is determined by the multi-department logic below.
            if (isAdmin && filter.AssignToId.HasValue && filter.AssignToId.Value != Guid.Empty)
            {
                query = query.Where(r => r.assignToId == filter.AssignToId.Value);
            }

            // DepartmentId filter: for admins, apply directly; for non-admins, handled inside multi-department logic
            if (isAdmin && filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == filter.DepartmentId.Value);
            }

            if (!isAdmin)
            {
                // Get all departments where engineer is a TeamLead (via EngineerDepartments)
                var teamLeadDeptIds = await _db.EngineerDepartments
                    .Where(ed => ed.EngineerId == engineer.Id && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
                    .Select(ed => ed.DepartmentId)
                    .ToListAsync(cancellationToken);

                // Fallback: check via legacy DepartmentId + global UserRoles
                if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
                {
                    var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                    join role in _db.Roles on userRole.RoleId equals role.Id
                                                    where userRole.UserId == engineer.ApplicationUserId
                                                          && role.Name == RoleNames.Teamleadengineer
                                                    select role.Id).AnyAsync(cancellationToken);
                    if (isTeamLeadViaRoles)
                        teamLeadDeptIds.Add(engineer.DepartmentId.Value);
                }

                // Get all departments this engineer belongs to
                var allEngineerDeptIds = await _db.EngineerDepartments
                    .Where(ed => ed.EngineerId == engineer.Id)
                    .Select(ed => ed.DepartmentId)
                    .ToListAsync(cancellationToken);

                // Include the active department if set
                if (engineer.DepartmentId.HasValue && !allEngineerDeptIds.Contains(engineer.DepartmentId.Value))
                    allEngineerDeptIds.Add(engineer.DepartmentId.Value);

                // If DepartmentId filter is provided, narrow down to only that department (within allowed departments)
                if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
                {
                    var filterDeptId = filter.DepartmentId.Value;
                    teamLeadDeptIds = teamLeadDeptIds.Where(d => d == filterDeptId).ToList();
                    allEngineerDeptIds = allEngineerDeptIds.Where(d => d == filterDeptId).ToList();
                }

                bool isTeamLead = teamLeadDeptIds.Any();

                if (isTeamLead)
                {
                    // Team lead: see all requests in departments where they are TeamLead,
                    // plus requests in other departments assigned to them, plus requests they created
                    query = query.Where(r =>
                        (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                        || r.assignToId == engineer.Id
                        || r.EngineerId == engineer.Id);
                }
                else if (allEngineerDeptIds.Any())
                {
                    // Non-teamlead engineer with departments: for each department check if it has a teamlead
                    var deptsWithoutTeamLead = new List<Guid>();
                    var deptsWithTeamLead = new List<Guid>();
                    foreach (var deptId in allEngineerDeptIds)
                    {
                        if (!await DepartmentHasTeamLeadAsync(deptId))
                            deptsWithoutTeamLead.Add(deptId);
                        else
                            deptsWithTeamLead.Add(deptId);
                    }

                    query = query.Where(r =>
                        // Departments without teamlead: member sees unassigned requests + requests assigned to them
                        (r.DepartmentId.HasValue && deptsWithoutTeamLead.Contains(r.DepartmentId.Value)
                            && (r.assignToId == null || r.assignToId == Guid.Empty || r.assignToId == engineer.Id))
                        // Departments with teamlead: member only sees requests assigned to them
                        || (r.DepartmentId.HasValue && deptsWithTeamLead.Contains(r.DepartmentId.Value) && r.assignToId == engineer.Id)
                        || r.assignToId == engineer.Id
                        || r.EngineerId == engineer.Id);
                }
                else
                {
                    // Engineer with no department assignments: requests assigned to them OR created by them
                    query = query.Where(r => r.assignToId == engineer.Id || r.EngineerId == engineer.Id);
                }
            }
            // Admin/SuperAdmin: no role-based filter applied — sees all requests (only filtered by data params above)

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var requestDtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);

            return new PaginatedList<GetAllEngineerRequestDto>(
                requestDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    // ---------------- GET REQUESTS BY STATUS FOR ENGINEER ----------------
    public async Task<PaginatedList<GetAllEngineerRequestDto>> GetRequestsByStatusForEngineerAsync(
        GetRequestsByStatusFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var engineer = await _db.Engineers
                .Include(x => x.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

            if (engineer == null)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            // Get all departments where engineer is a TeamLead (via EngineerDepartments)
            var teamLeadDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineer.Id && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
                .Select(ed => ed.DepartmentId)
                .ToListAsync(cancellationToken);

            // Fallback: check via legacy DepartmentId + global UserRoles
            if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
            {
                var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                join role in _db.Roles on userRole.RoleId equals role.Id
                                                where userRole.UserId == engineer.ApplicationUserId
                                                      && role.Name == RoleNames.Teamleadengineer
                                                select role.Id).AnyAsync(cancellationToken);
                if (isTeamLeadViaRoles)
                    teamLeadDeptIds.Add(engineer.DepartmentId.Value);
            }

            // Get all departments this engineer belongs to
            var allEngineerDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineer.Id)
                .Select(ed => ed.DepartmentId)
                .ToListAsync(cancellationToken);

            // Include the active department if set
            if (engineer.DepartmentId.HasValue && !allEngineerDeptIds.Contains(engineer.DepartmentId.Value))
                allEngineerDeptIds.Add(engineer.DepartmentId.Value);

            bool isTeamLead = teamLeadDeptIds.Any();

            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.EngineerRequestAttachments)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.Department)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.SpecialFieldValues)
                    .ThenInclude(v => v.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .AsSplitQuery()
                .AsNoTracking();

            // Filter by status
            query = query.Where(r => r.StatusId == filter.StatusId);

            // Filter based on multi-department team lead status
            if (isTeamLead)
            {
                // Team lead: see all requests in departments where they are TeamLead,
                // plus requests assigned to them, plus requests they created
                query = query.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.assignToId == engineer.Id
                    || r.EngineerId == engineer.Id);
            }
            else if (allEngineerDeptIds.Any())
            {
                var deptsWithoutTeamLead = new List<Guid>();
                var deptsWithTeamLead = new List<Guid>();
                foreach (var deptId in allEngineerDeptIds)
                {
                    if (!await DepartmentHasTeamLeadAsync(deptId))
                        deptsWithoutTeamLead.Add(deptId);
                    else
                        deptsWithTeamLead.Add(deptId);
                }

                query = query.Where(r =>
                    // Departments without teamlead: member sees unassigned requests + requests assigned to them
                    (r.DepartmentId.HasValue && deptsWithoutTeamLead.Contains(r.DepartmentId.Value)
                        && (r.assignToId == null || r.assignToId == Guid.Empty || r.assignToId == engineer.Id))
                    // Departments with teamlead: member only sees requests assigned to them
                    || (r.DepartmentId.HasValue && deptsWithTeamLead.Contains(r.DepartmentId.Value) && r.assignToId == engineer.Id)
                    || r.assignToId == engineer.Id
                    || r.EngineerId == engineer.Id);
            }
            else
            {
                // Engineer with no department assignments: requests assigned to them OR created by them
                query = query.Where(r => r.assignToId == engineer.Id || r.EngineerId == engineer.Id);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var requestDtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);

            return new PaginatedList<GetAllEngineerRequestDto>(
                requestDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    public async Task<bool> DepartmentHasTeamLeadAsync(Guid departmentId)
    {
        var teamleadRoleName = RoleNames.Teamleadengineer;

        // Check via EngineerDepartments (multi-department role assignment)
        var hasViaDepRole = await _db.EngineerDepartments
            .AnyAsync(ed => ed.DepartmentId == departmentId
                         && ed.Role != null && ed.Role.Name == teamleadRoleName);

        if (hasViaDepRole) return true;

        // Fallback: check via legacy DepartmentId + global UserRoles
        return await (from eng in _db.Engineers
                      join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                      join role in _db.Roles on userRole.RoleId equals role.Id
                      where eng.DepartmentId == departmentId && role.Name == teamleadRoleName
                      select eng.Id).AnyAsync();
    }

    // ---------------- GET REQUEST ACTIVITIES ----------------
    public async Task<List<GetEngineerRequestActiviteDto>> GetRequestActivitiesAsync(Guid requestId)
    {
        var activities = await _db.EngineerRequestActivites
            .Include(a => a.Engineer)
            .Include(a => a.Status)
            .Where(a => a.EngineerRequestId == requestId)
            .OrderByDescending(a => a.CreatedDate)
            .AsNoTracking()
            .ToListAsync();

        var activitiesDto = activities.Select(a => new GetEngineerRequestActiviteDto
        {
            Id = a.Id,
            EngineerRequestId = a.EngineerRequestId,
            EngineerId = a.EngineerId,
            EngineerName = a.Engineer != null ? $"{a.Engineer.nameEn} / {a.Engineer.nameAr}" : null,
            StatusId = a.StatusId,
            StatusName = a.Status != null ? $"{a.Status.nameEn} / {a.Status.nameAr}" : null,
            ActionType = a.ActionType,
            CreatedDate = a.CreatedDate
        }).ToList();

        return activitiesDto;
    }

    // ---------------- GET ENGINEER REQUEST COUNT BY STATUS ----------------
    public async Task<List<GetEngineerRequestCountByStatusDto>> GetEngineerRequestCountByStatusAsync(Guid engineerId)
    {
        // Load engineer to determine department and manager status
        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == engineerId);

        // Get all departments where engineer is a TeamLead (via EngineerDepartments)
        var teamLeadDeptIds = await _db.EngineerDepartments
            .Where(ed => ed.EngineerId == engineerId && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
            .Select(ed => ed.DepartmentId)
            .ToListAsync();

        // Fallback: check via legacy DepartmentId + global UserRoles
        if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
        {
            var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                            join role in _db.Roles on userRole.RoleId equals role.Id
                                            where userRole.UserId == engineer.ApplicationUserId
                                                  && role.Name == RoleNames.Teamleadengineer
                                            select role.Id).AnyAsync();
            if (isTeamLeadViaRoles)
                teamLeadDeptIds.Add(engineer.DepartmentId.Value);
        }

        // Get all departments this engineer belongs to
        var allEngineerDeptIds = await _db.EngineerDepartments
            .Where(ed => ed.EngineerId == engineerId)
            .Select(ed => ed.DepartmentId)
            .ToListAsync();

        // Include the active department if set
        if (engineer.DepartmentId.HasValue && !allEngineerDeptIds.Contains(engineer.DepartmentId.Value))
            allEngineerDeptIds.Add(engineer.DepartmentId.Value);

        bool isTeamLead = teamLeadDeptIds.Any();

        IQueryable<EngineerRequest> query = _db.EngineerRequests.Include(er => er.Status);

        if (isTeamLead)
        {
            // Team lead: count all requests in departments where they are TeamLead,
            // plus requests assigned to them, plus requests they created
            query = query.Where(er =>
                (er.DepartmentId.HasValue && teamLeadDeptIds.Contains(er.DepartmentId.Value))
                || er.assignToId == engineerId
                || er.EngineerId == engineerId);
        }
        else if (allEngineerDeptIds.Any())
        {
            var deptsWithoutTeamLead = new List<Guid>();
            foreach (var deptId in allEngineerDeptIds)
            {
                if (!await DepartmentHasTeamLeadAsync(deptId))
                    deptsWithoutTeamLead.Add(deptId);
            }

            var deptsWithTeamLead = new List<Guid>();
            foreach (var deptId in allEngineerDeptIds)
            {
                if (deptsWithoutTeamLead.Contains(deptId)) continue;
                deptsWithTeamLead.Add(deptId);
            }

            query = query.Where(er =>
                // Departments without teamlead: member sees unassigned requests + requests assigned to them
                (er.DepartmentId.HasValue && deptsWithoutTeamLead.Contains(er.DepartmentId.Value)
                    && (er.assignToId == null || er.assignToId == Guid.Empty || er.assignToId == engineerId))
                // Departments with teamlead: member only sees requests assigned to them
                || (er.DepartmentId.HasValue && deptsWithTeamLead.Contains(er.DepartmentId.Value) && er.assignToId == engineerId)
                || er.assignToId == engineerId
                || er.EngineerId == engineerId);
        }
        else
        {
            // Engineer with no department assignments: count requests created by them OR assigned to them
            query = query.Where(er => er.EngineerId == engineerId || er.assignToId == engineerId);
        }

        // Get all statuses
        var allStatuses = await _db.Statuses
            .AsNoTracking()
            .Select(s => new { s.Id, s.nameEn, s.nameAr })
            .ToListAsync();

        // Get request counts grouped by status
        var requestCountsFromDb = await query
            .GroupBy(er => er.StatusId)
            .Select(g => new
            {
                StatusId = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        // Create dictionary for quick lookup
        var countDict = requestCountsFromDb.ToDictionary(x => x.StatusId, x => x.Count);

        // Return all statuses with their counts (0 if no requests)
        var requestCounts = allStatuses
            .Select(s => new GetEngineerRequestCountByStatusDto
            {
                StatusId = s.Id,
                StatusName = s.nameEn,
                StatusNameAr = s.nameAr ?? string.Empty,
                Count = countDict.ContainsKey(s.Id) ? countDict[s.Id] : 0
            })
            .OrderBy(x => x.StatusName)
            .ToList();

        return requestCounts;
    }

    public async Task ProcessScheduledStatusUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var now = Contracting.Shared.Common.DateTimeHelper.DateTimeNow;

        var statuses = await _db.Statuses
            .AsNoTracking()
            .Select(s => new StatusKeywordProjection(s.Id, s.nameEn, s.nameAr, s.Code))
            .ToListAsync(cancellationToken);

        var inProgressStatusId = FindStatusIdByKeywords(statuses, InProgressKeywords);
        var delayedStatusId = FindStatusIdByKeywords(statuses, DelayedKeywords);
        var completedStatusIds = ExtractStatusIds(statuses, CompletedKeywords);
        var newPendingStatusIds = ExtractStatusIds(statuses, NewPendingKeywords);

        var activities = new List<EngineerRequestActivite>();

        // Move requests to InProgress when they have planning dates.
        // This covers requests whose start date has arrived and also requests with future planned dates.
        if (inProgressStatusId.HasValue && newPendingStatusIds.Count > 0)
        {
            var startCandidates = await _db.EngineerRequests
                .Where(r => r.startDate.HasValue
                            || (r.endDate.HasValue && r.endDate.Value > now))
                .Where(r => newPendingStatusIds.Contains(r.StatusId))
                .Where(r => r.startDate.HasValue
                            || r.endDate.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var request in startCandidates)
            {
                request.StatusId = inProgressStatusId.Value;
                activities.Add(CreateAutomaticActivity(request, inProgressStatusId.Value));
            }
        }

        // For endDate: Only change to Delayed if current status is InProgress
        // This ensures we only auto-update once when the end date passes
        // If user changes status after delay, it won't be changed back to Delayed
        if (delayedStatusId.HasValue && inProgressStatusId.HasValue)
        {
            var delayCandidates = await _db.EngineerRequests
                .Where(r => r.endDate.HasValue
                            && r.endDate.Value <= now
                            && r.StatusId == inProgressStatusId.Value
                            && !completedStatusIds.Contains(r.StatusId))
                .ToListAsync(cancellationToken);

            foreach (var request in delayCandidates)
            {
                request.StatusId = delayedStatusId.Value;
                activities.Add(CreateAutomaticActivity(request, delayedStatusId.Value));
            }
        }

        if (activities.Count == 0)
        {
            return;
        }

        await _db.EngineerRequestActivites.AddRangeAsync(activities, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static EngineerRequestActivite CreateAutomaticActivity(EngineerRequest request, Guid statusId)
    {
        return new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = request.assignToId ?? request.EngineerId,
            StatusId = statusId,
            ActionType = AutomaticStatusActionType
        };
    }

    private static Guid? FindStatusIdByKeywords(IEnumerable<StatusKeywordProjection> statuses, string[] keywords)
    {
        return statuses
            .FirstOrDefault(s => HasKeyword(s.Code, keywords)
                              || HasKeyword(s.NameEn, keywords)
                              || HasKeyword(s.NameAr, keywords))?.Id;
    }

    private static HashSet<Guid> ExtractStatusIds(IEnumerable<StatusKeywordProjection> statuses, string[] keywords)
    {
        return statuses
            .Where(s => HasKeyword(s.Code, keywords)
                     || HasKeyword(s.NameEn, keywords)
                     || HasKeyword(s.NameAr, keywords))
            .Select(s => s.Id)
            .ToHashSet();
    }

    private static bool HasKeyword(string? value, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();
        return keywords.Any(keyword => normalized.Contains(keyword));
    }

    private sealed record StatusKeywordProjection(Guid Id, string? NameEn, string? NameAr, string? Code);

    public async Task<ErrorOr<bool>> ConfirmDeliveryDateAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return Error.NotFound("Request.NotFound", _localizer[SharedResourcesKeys.RequestNotFound]);

        if (request.endDate is null)
            return Error.Validation("Request.NoDeliveryDate", "Cannot confirm delivery date: no end date is set on this request.");

        if (request.IsDeliveryDateConfirmed)
            return Error.Conflict("Request.AlreadyConfirmed", "Delivery date is already confirmed.");

        request.IsDeliveryDateConfirmed = true;
        await _db.SaveChangesAsync();
        return true;
    }
}
