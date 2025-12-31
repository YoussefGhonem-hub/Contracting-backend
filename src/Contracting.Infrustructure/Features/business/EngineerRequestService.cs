using Contracting.Shared.Resources;
using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.master;
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
using Contracting.Shared.HelperDtos;
using MapsterMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Contracting.Infrustructure.Features.business
{
    public class EngineerRequestService : IEngineerRequestService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResources> _localizer;


        public EngineerRequestService(ApplicationDbContext db, IMapper mapper, INotificationService notificationService, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _notificationService = notificationService;
            _localizer = localizer;
        }

        // ---------------- CREATE ----------------
        public async Task<GetAllEngineerRequestDto> CreateEngineerRequestAsync(CreateEngineerRequestDto dto)
        {
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
            var teamLead = await (from Engineer in _db.Engineers
                                  join userRole in _db.UserRoles on engineer.ApplicationUserId equals userRole.UserId
                                  join role in _db.Roles on userRole.RoleId equals role.Id
                                  where engineer.DepartmentId == request.DepartmentId && role.Name == RoleNames.Teamleadengineer
                                  select engineer.ApplicationUserId)
                      .FirstOrDefaultAsync();

            // Handle notes
            if (dto.EngineerRequestNotes != null && dto.EngineerRequestNotes.Any())
            {
                request.EngineerRequestNotes = dto.EngineerRequestNotes
                    .Select(noteDto =>
                    {
                        var note = _mapper.Map<EngineerRequestNotes>(noteDto);
                        note.EngineerId = engineer?.Id;
                        note.EngineerRequest = request;
                        return note;
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

            // Reload with navigation properties
            var createdRequest = await _db.EngineerRequests
                                .Include(r => r.Project)
                                .Include(r => r.Department)
                                .Include(r => r.Priority)
                                .Include(r => r.Status)
                                .Include(r => r.Engineer)
                                    .ThenInclude(e => e.Department)
                                .Include(r => r.EngineerRequestNotes)
                                .Include(r=>r.EngineerRequestActivites)
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r => r.Id == request.Id);

            // Send notification to the Team Lead
            if (teamLead != Guid.Empty)
            {
                // Get all FCM tokens for the team lead
                var teamLeadTokens = await _db.userDeviceTokens
                    .Where(t => t.UserId == teamLead)
                    .Select(t => t.FcmToken)
                    .ToListAsync();

                foreach (var token in teamLeadTokens)
                {
                    var notification = new PushNotificationDto
                    {
                        Token = token,
                        Title = "New Request Created",
                        Body = $"A new request has been created for your department {createdRequest.Id}.",
                        DepartmentId = request.DepartmentId?.ToString(),
                        RequestId = request.Id.ToString()
                    };
                    await _notificationService.SendAsync(notification);
                }
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
                        }
                    }
                    else
                    {
                        // Add new note
                        var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                        newNote.EngineerRequestId = request.Id;
                        newNote.EngineerId = engineer?.Id;
                        await _db.EngineerRequestNotes.AddAsync(newNote);
                    }
                }
            }

            await _db.SaveChangesAsync();

            // Reload with navigation properties
            var updatedRequest = await _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.EngineerRequestNotes)
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
            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r=>r.Status)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.EngineerRequestNotes)
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
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);
          
            // Get department manager
            var departmentId = request.DepartmentId;
            if (!departmentId.HasValue)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNoDepartment]);

            var isManager = await (from eng in _db.Engineers
                                   join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                   join role in _db.Roles on userRole.RoleId equals role.Id
                                   where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                                   select eng.Id)
                       .AnyAsync();

            var isAssigned = request.assignToId.HasValue && request.assignToId.Value == currentUserId;

            // Only manager or assigned engineer can take action
            if (!isManager && !isAssigned)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);

            // Track previous status for activity
            var previousStatusId = request.StatusId;
            var currentEngineerId = await _db.Engineers
                .Where(e => e.ApplicationUserId == currentUserId)
                .Select(e => e.Id)
                .FirstOrDefaultAsync();

            // If manager, allow assignment
            if (isManager && actionDto.assignToId.HasValue)
            {
                var previousAssignedId = request.assignToId;
                request.assignToId = actionDto.assignToId.Value;
                
                // Create activity for assignment
                if (previousAssignedId != actionDto.assignToId.Value)
                {
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
            else if (isAssigned && actionDto.assignToId.HasValue && actionDto.assignToId.Value != currentUserId)
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

            if (isAssigned && request.timeDuration != actionDto.timeDuration.Value)
            {
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.TimeDurationMismatch]);
            }

            if (actionDto.timeDuration.HasValue)
            {
                request.timeDuration = actionDto.timeDuration.Value;
                request.startDate = actionDto.startDate.Value;
                request.endDate = actionDto.startDate.Value.AddDays(actionDto.timeDuration.Value);
            }                        

            if (actionDto.EngineerRequestNotes != null && actionDto.EngineerRequestNotes.Any())
            {
                foreach (var noteDto in actionDto.EngineerRequestNotes)
                {
                    var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                    newNote.EngineerRequestId = request.Id;
                    await _db.EngineerRequestNotes.AddAsync(newNote);
                }
            }

            await _db.SaveChangesAsync();

            if (isManager && request.EngineerId.HasValue)
            {
                if (request.Engineer.ApplicationUserId != Guid.Empty)
                {
                    var creatorTokens = await _db.userDeviceTokens
                                       .Where(t => t.UserId == request.Engineer.ApplicationUserId)
                                       .Select(t => t.FcmToken)
                                       .ToListAsync();
                    foreach (var token in creatorTokens)
                    {
                        var notification = new PushNotificationDto
                        {
                            Token = token,
                            Title = "Your Request Was Updated",
                            Body = "The team lead has taken action on your request.",
                            EngineerId = request.Engineer.ApplicationUserId.ToString(),
                            RequestId = request.Id.ToString()
                        };
                        await _notificationService.SendAsync(notification);
                    }
                }
            }

            if (request.assignToId.HasValue)
            {
                var assignedTokens = await _db.userDeviceTokens
                    .Where(t => t.UserId == request.assignToId.Value)
                    .Select(t => t.FcmToken)
                    .ToListAsync();

                foreach (var token in assignedTokens)
                {
                    var notification = new PushNotificationDto
                    {
                        Token = token,
                        Title = "You Have Been Assigned a Request",
                        Body = "A request has been assigned to you.",
                        EngineerId = request.assignToId.Value.ToString(),
                        RequestId = request.Id.ToString()
                    };
                    await _notificationService.SendAsync(notification);
                }
            }

            // If the current user is the assigned engineer and not the team lead
            if (isAssigned && request.EngineerId.HasValue)
            {
                var creator = await _db.Engineers
                    .Where(e => e.Id == request.EngineerId.Value)
                    .Select(e => e.ApplicationUserId)
                    .FirstOrDefaultAsync();

                if (creator != Guid.Empty)
                {
                    var creatorTokens = await _db.userDeviceTokens
                        .Where(t => t.UserId == creator)
                        .Select(t => t.FcmToken)
                        .ToListAsync();

                    foreach (var token in creatorTokens)
                    {
                        var notification = new PushNotificationDto
                        {
                            Token = token,
                            Title = "Update on Your Request",
                            Body = "The assigned engineer has taken action on your request.",
                            EngineerId = creator.ToString(),
                            RequestId = request.Id.ToString()
                        };
                        await _notificationService.SendAsync(notification);
                    }
                }
            }


            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.ActionTakenSuccess]);
        }



        public async Task<PaginatedList<GetAllEngineerRequestDto>> GetCreatedRequestOrapplaied(BaseFilterDto filter, CancellationToken cancellationToken = default)
        {
            var engineer = _db.Engineers.Include(x => x.ApplicationUser).AsNoTracking().FirstOrDefault(x => x.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.EngineerRequestNotes)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Where(r => r.Engineer.ApplicationUser.Id == Guid.Parse(CurrentUser.UserId) || r.assignToId == engineer.Id)
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
    }
}