using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.ProjectDtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public ProjectService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<GetProjectDto> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = _mapper.Map<Project>(dto);

            if (project.BranchId == Guid.Empty || project.BranchId == null)
            {
                project.BranchId = null;
            }

            await _db.Projects.AddAsync(project);
            await _db.SaveChangesAsync();

            return _mapper.Map<GetProjectDto>(project);
        }

        public async Task<GetProjectDto> UpdateProjectAsync(UpdateProjectDto dto)
        {
            var project = await _db.Projects
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (project is null)
                return null!;

            project.nameEn = dto.nameEn;
            project.nameAr = dto.nameAr;
            project.location = dto.location;
            project.Code = dto.Code;

            if (dto.BranchId == Guid.Empty || dto.BranchId == null)
            {
                project.BranchId = null;
            }
            else
            {
                project.BranchId = dto.BranchId;
            }

            await _db.SaveChangesAsync();

            return _mapper.Map<GetProjectDto>(project);
        }

        public async Task<bool> DeleteProjectAsync(Guid projectId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project is null)
                return false;

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return true;
        }

                // ✅ UPDATED: Filter by branchId
        public async Task<PaginatedList<GetProjectDto>> GetAllProjectsAsync(
            Guid? branchId,
            BaseFilterDto filter,
            CancellationToken cancellationToken = default)
        {
            var query = _db.Projects
                .Include(p => p.Branch)
                .AsNoTracking();

            // ✅ ADDED: Filter by BranchId if provided
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderBy(p => p.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var projects = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var projectDtos = _mapper.Map<List<GetProjectDto>>(projects);

            return new PaginatedList<GetProjectDto>(
                projectDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<GetProjectDto> GetProjectByIdAsync(Guid projectId)
        {
            var project = await _db.Projects
                .Include(p => p.Branch)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == projectId);

            return project is null ? null! : _mapper.Map<GetProjectDto>(project);
        }

        // ✅ UPDATED: Filter by branchId
        public async Task<List<GetProjectDropDownDto>> GetProjectDropdownAsync(Guid? branchId)
        {
            var query = _db.Projects
                .Include(p => p.Branch)
                .AsNoTracking();

            // ✅ ADDED: Filter by BranchId if provided
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }

            var projects = await query.ToListAsync();

            return _mapper.Map<List<GetProjectDropDownDto>>(projects);
        }
    }
}