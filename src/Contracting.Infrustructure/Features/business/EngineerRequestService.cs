using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
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
            var request = _mapper.Map<EngineerRequest>(dto);

            // Handle empty GUIDs
            if (request.ProjectId == Guid.Empty) request.ProjectId = null;
            if (request.DepartmentId == Guid.Empty) request.DepartmentId = null;
            if (request.PriorityId == Guid.Empty) request.PriorityId = null;
            if (request.EngineerId == Guid.Empty) request.EngineerId = null;

            await _db.EngineerRequests.AddAsync(request);
            await _db.SaveChangesAsync();

            // Reload with navigation properties
            var createdRequest = await _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == request.Id);

            return _mapper.Map<GetAllEngineerRequestDto>(createdRequest);
        }

        // ---------------- UPDATE ----------------
        public async Task<GetAllEngineerRequestDto> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto)
        {
            var request = await _db.EngineerRequests.FindAsync(dto.Id);
            if (request is null)
                return null!;

            // Check if action has been taken (by checking if Note is set with NoteDate)
            if (request.NoteDate.HasValue)
            {
                // Request has been actioned, cannot update
                return null!;
            }

            // Update fields
            request.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            request.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            request.PriorityId = dto.PriorityId == Guid.Empty ? null : dto.PriorityId;
            request.EngineerId = dto.EngineerId == Guid.Empty ? null : dto.EngineerId;
            request.Descreption = dto.Descreption;

            await _db.SaveChangesAsync();

            // Reload with navigation properties
            var updatedRequest = await _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
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
            if (request.NoteDate.HasValue)
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
        public async Task<bool> TakeActionOnRequestAsync(Guid requestId, Guid engineerId, bool isApproved, string? actionNote)
        {
            var request = await _db.EngineerRequests.FindAsync(requestId);
            if (request is null)
                return false;

            // Check if action already taken
            if (request.NoteDate.HasValue)
                return false;

            // Verify engineer is manager of the department
            if (request.DepartmentId.HasValue)
            {
                var isManager = await IsEngineerManagerOfDepartmentAsync(engineerId, request.DepartmentId.Value);
                if (!isManager)
                    return false;
            }
            else
            {
                return false; // No department assigned
            }

            // Update request with action
            request.Note = actionNote ?? (isApproved ? "Approved" : "Rejected");
            request.NoteDate = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // TODO: Create entry in action/history table if needed
            // await _db.RequestActions.AddAsync(new RequestAction { ... });

            return true;
        }
    }
}