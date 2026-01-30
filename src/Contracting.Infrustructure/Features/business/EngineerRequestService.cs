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

        // Find the Team Lead for the selected department
        var teamLead = await (from eng in _db.Engineers
                      join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                      join role in _db.Roles on userRole.RoleId equals role.Id
                      where eng.DepartmentId == request.DepartmentId && role.Name == RoleNames.Teamleadengineer
                      select eng.ApplicationUserId)
              .FirstOrDefaultAsync();

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
                            .AsNoTracking()
                            .FirstOrDefaultAsync(r => r.Id == request.Id);


        if (teamLead == Guid.Empty)
        {
            // No team lead found, notify all engineers in the department
            var departmentEngineers = await _db.Engineers
                .Where(e => e.DepartmentId == request.DepartmentId)
                .Select(e => e.ApplicationUserId)
                .ToListAsync();
            foreach (var engineerUserId in departmentEngineers)
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
    public async Task<GetAllEngineerRequestDto> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto)
    {
        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        var request = await _db.EngineerRequests
            .Include(r => r.EngineerRequestNotes)
            .FirstOrDefaultAsync(r => r.Id == dto.Id);
        if (request is null)
            return null!;

        if (request.assignToId != null && request.assignToId != Guid.Empty)
            return null!;

        // Update fields
        request.ProjectId = dto.ProjectId == Guid.Empty ? request.ProjectId : dto.ProjectId;
        request.DepartmentId = dto.DepartmentId == Guid.Empty ? request.DepartmentId : dto.DepartmentId;
        request.PriorityId = dto.PriorityId == Guid.Empty ? request.PriorityId : dto.PriorityId;
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

        await _db.SaveChangesAsync();

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
                .Where(r => r.DepartmentId == departmentId)
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
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == requestId);

        return request is null ? null! : _mapper.Map<GetAllEngineerRequestDto>(request);
    }

    // ---------------- CHECK IF ENGINEER IS MANAGER ----------------
    public async Task<bool> IsEngineerManagerOfDepartmentAsync(Guid engineerId, Guid departmentId)
    {
        var isTeamLead = await (from eng in _db.Engineers
                                join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                join role in _db.Roles on userRole.RoleId equals role.Id
                                where eng.Id == engineerId && eng.DepartmentId == departmentId && role.Name == RoleNames.Teamleadengineer
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
            .FirstOrDefaultAsync(r => r.Id == requestId);


        var currentEngineerId = await _db.Engineers
            .Where(e => e.ApplicationUserId == currentUserId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);
      
        // Get department manager
        var departmentId = request.DepartmentId;
        if (!departmentId.HasValue)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNoDepartment]);

        // Check if department has a team lead
        bool departmentHasTeamLead = await DepartmentHasTeamLeadAsync(departmentId.Value);

        var isManager = await (from eng in _db.Engineers
                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                               join role in _db.Roles on userRole.RoleId equals role.Id
                               where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                               select eng.Id)
                   .AnyAsync();

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
            var isEngineerInDepartment = await _db.Engineers.AnyAsync(e => e.ApplicationUserId == currentUserId && e.DepartmentId == departmentId.Value);
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

        var isManager = await (from eng in _db.Engineers
                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                               join role in _db.Roles on userRole.RoleId equals role.Id
                               where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                               select eng.Id)
                   .AnyAsync();

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



    public async Task<PaginatedList<GetAllEngineerRequestDto>> GetCreatedRequestOrapplaied(BaseFilterDto filter, CancellationToken cancellationToken = default)
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


            // Check if user is a team lead
            var isTeamLead = await (from eng in _db.Engineers
                                   join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                   join role in _db.Roles on userRole.RoleId equals role.Id
                                   where eng.Id == engineer.Id && role.Name == RoleNames.Teamleadengineer
                                   select eng.Id)
                       .AnyAsync();

            // Check if department has a team lead
            bool departmentHasTeamLead = false;
            if (engineer.DepartmentId.HasValue)
            {
                departmentHasTeamLead = await DepartmentHasTeamLeadAsync(engineer.DepartmentId.Value);
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
                .AsNoTracking();

            if (isTeamLead && engineer.DepartmentId.HasValue)
            {
                // Team lead: see all requests in their department OR requests they created
                query = query.Where(r => r.DepartmentId == engineer.DepartmentId.Value || r.EngineerId == engineer.Id);
            }
            else if (!departmentHasTeamLead && engineer.DepartmentId.HasValue)
            {
                // No team lead: show requests in department (if assignToId is null/empty or assigned to them), OR requests they created
                query = query.Where(r => 
                    (r.DepartmentId == engineer.DepartmentId.Value && 
                        (r.assignToId == null || r.assignToId == Guid.Empty || r.assignToId == engineer.Id)) 
                    || r.EngineerId == engineer.Id);
            }
            else
            {
                // Regular engineer: requests assigned to them OR requests they created
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
        return await (from eng in _db.Engineers
                      join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                      join role in _db.Roles on userRole.RoleId equals role.Id
                      where eng.DepartmentId == departmentId && role.Name == RoleNames.Teamleadengineer
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

        bool isTeamLeadForDepartment = false;
        if (engineer.DepartmentId.HasValue)
        {
            isTeamLeadForDepartment = await IsEngineerManagerOfDepartmentAsync(engineerId, engineer.DepartmentId.Value);
        }

        IQueryable<EngineerRequest> query = _db.EngineerRequests;

        if (isTeamLeadForDepartment && engineer.DepartmentId.HasValue)
        {
            // Department manager: count all requests under their department
            query = query.Where(er => er.DepartmentId == engineer.DepartmentId.Value);
        }
        else
        {
            // Regular engineer: count only own or assigned requests
            query = query.Where(er => er.EngineerId == engineerId || er.assignToId == engineerId);
        }

        var requestCounts = await query
            .GroupBy(er => new { er.StatusId, er.Status.nameEn })
            .Select(g => new GetEngineerRequestCountByStatusDto
            {
                StatusId = g.Key.StatusId,
                StatusName = g.Key.nameEn,
                Count = g.Count()
            })
            .OrderBy(x => x.StatusName)
            .AsNoTracking()
            .ToListAsync();

        return requestCounts;
    }

    public async Task ProcessScheduledStatusUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var statuses = await _db.Statuses
            .AsNoTracking()
            .Select(s => new StatusKeywordProjection(s.Id, s.nameEn, s.nameAr, s.Code))
            .ToListAsync(cancellationToken);

        var inProgressStatusId = FindStatusIdByKeywords(statuses, InProgressKeywords);
        var delayedStatusId = FindStatusIdByKeywords(statuses, DelayedKeywords);
        var completedStatusIds = ExtractStatusIds(statuses, CompletedKeywords);

        var activities = new List<EngineerRequestActivite>();

        if (inProgressStatusId.HasValue)
        {
            var startCandidates = await _db.EngineerRequests
                .Where(r => r.startDate.HasValue
                            && r.startDate.Value <= now
                            && r.StatusId != inProgressStatusId.Value)
                .ToListAsync(cancellationToken);

            foreach (var request in startCandidates)
            {
                request.StatusId = inProgressStatusId.Value;
                activities.Add(CreateAutomaticActivity(request, inProgressStatusId.Value));
            }
        }

        if (delayedStatusId.HasValue)
        {
            var delayCandidates = await _db.EngineerRequests
                .Where(r => r.endDate.HasValue
                            && r.endDate.Value <= now
                            && r.StatusId != delayedStatusId.Value
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
}
