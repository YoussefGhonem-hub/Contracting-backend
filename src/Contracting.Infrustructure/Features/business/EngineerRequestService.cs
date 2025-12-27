using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business
{
    public class EngineerRequestService : IEngineerRequestService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public EngineerRequestService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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

            // Handle notes
            if (dto.EngineerRequestNotes != null && dto.EngineerRequestNotes.Any())
            {
                request.EngineerRequestNotes = dto.EngineerRequestNotes
                    .Select(noteDto =>
                    {
                        var note = _mapper.Map<EngineerRequestNotes>(noteDto);
                        note.StatusId = firstStatus?.Id ?? Guid.Empty;
                        note.EngineerId = engineer?.Id;
                        note.EngineerRequest = request;
                        return note;
                    }).ToList();
            }

            await _db.EngineerRequests.AddAsync(request);
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
                                .AsNoTracking()
                                .FirstOrDefaultAsync(r => r.Id == request.Id);

            return _mapper.Map<GetAllEngineerRequestDto>(createdRequest);
        }

        // ---------------- UPDATE ----------------
        public async Task<GetAllEngineerRequestDto> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto)
        {
            var request = await _db.EngineerRequests
                .Include(r => r.EngineerRequestNotes)
                .FirstOrDefaultAsync(r => r.Id == dto.Id);
            if (request is null)
                return null!;

            if (request.NoteDate.HasValue || request.assignToId != Guid.Empty)
                return null!;

            // Update fields
            request.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            request.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            request.PriorityId = dto.PriorityId == Guid.Empty ? null : dto.PriorityId;
            request.EngineerId = dto.EngineerId == Guid.Empty ? null : dto.EngineerId;
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
                        var existingNote = request.EngineerRequestNotes.FirstOrDefault(n => n.Id == noteDto.Id);
                        if (existingNote != null)
                        {
                            _mapper.Map(noteDto, existingNote);
                        }
                    }
                    else
                    {
                        var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                        newNote.EngineerRequestId = request.Id;
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
        public async Task<bool> DeleteEngineerRequestAsync(Guid requestId)
        {
            var request = await _db.EngineerRequests.FindAsync(requestId);
            if (request is null)
                return false;

            // Check if action has been taken
            if (request.NoteDate.HasValue || request.assignToId != Guid.Empty)
            {
                // Request has been actioned, cannot delete
                return false;
            }

            _db.EngineerRequests.Remove(request);
            await _db.SaveChangesAsync();
            return true;
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
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == requestId);

            return request is null ? null! : _mapper.Map<GetAllEngineerRequestDto>(request);
        }

        // ---------------- CHECK IF ENGINEER IS MANAGER ----------------
        public async Task<bool> IsEngineerManagerOfDepartmentAsync(Guid engineerId, Guid departmentId)
        {
            var department = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == departmentId && d.isManager == true);

            return department is not null;
        }

        // ---------------- TAKE ACTION ON REQUEST ----------------
        public async Task<bool> TakeActionOnRequestAsync(Guid requestId, Guid currentUserId, TakeActionRequestDto actionDto)
        {

            var request = await _db.EngineerRequests
                .Include(r => r.EngineerRequestNotes)
                .FirstOrDefaultAsync(r => r.Id == requestId);

            if (request is null)
                return false;
          
            // Get department manager
            var departmentId = request.DepartmentId;
            if (!departmentId.HasValue)
                return false;

            var manager = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.DepartmentId == departmentId.Value && e.isManager);

            var isManager = manager != null && manager.ApplicationUserId == currentUserId;
            var isAssigned = request.assignToId.HasValue && request.assignToId.Value == currentUserId;

            // Only manager or assigned engineer can take action
            if (!isManager && !isAssigned)
                return false;

            // If manager, allow assignment
            if (isManager && actionDto.assignToId.HasValue)
            {
                request.assignToId = actionDto.assignToId.Value;
            }
            // If assigned engineer, do not allow assignment change
            else if (isAssigned && actionDto.assignToId.HasValue && actionDto.assignToId.Value != currentUserId)
            {
                // Assigned engineer cannot reassign
                return false;
            }

            // Update status, note, and note date
            if (actionDto.statusId.HasValue)
                request.StatusId = actionDto.statusId.Value;

            if (isAssigned && request.timeDuration != actionDto.timeDuration.Value)
            {
                return false;
            }

            if (actionDto.timeDuration.HasValue)
            {
                request.timeDuration = actionDto.timeDuration.Value;
            }            

            if (!request.NoteDate.HasValue)
            {            
               request.Note = (actionDto.isAprroved ? "Approved" : "Rejected");
               request.NoteDate = DateTime.UtcNow;
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
            return true;
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
    }
}