using Contracting.Shared.Resources;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common.Enums;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features
{
    public class ProjectService : IProjectService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly IStorageService _storageService;

        public ProjectService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer, IStorageService storageService)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
            _storageService = storageService;
        }

        public async Task<GetProjectDto> CreateProjectAsync(CreateProjectDto dto)
        {
            var project = _mapper.Map<Project>(dto);

            if (project.BranchId == Guid.Empty || project.BranchId == null)
            {
                project.BranchId = null;
            }

            // Handle image upload
            if (dto.Image != null)
            {
                var uploaded = await _storageService.Upload(dto.Image);
                if (uploaded != null)
                {
                    project.imageUrl = uploaded.Url;
                    project.imageKey = uploaded.Key;
                }
            }

            await _db.Projects.AddAsync(project);
            await _db.SaveChangesAsync();

            return await GetProjectByIdAsync(project.Id);
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
            project.Area = dto.Area;
            project.StartDate = dto.StartDate;
            project.ProjectStatus = dto.ProjectStatus;

            // Handle image upload
            if (dto.Image != null)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(project.imageKey))
                {
                    await _storageService.Delete(project.imageKey);
                }

                var uploaded = await _storageService.Upload(dto.Image);
                if (uploaded != null)
                {
                    project.imageUrl = uploaded.Url;
                    project.imageKey = uploaded.Key;
                }
            }

            if (dto.BranchId == Guid.Empty || dto.BranchId == null)
            {
                project.BranchId = null;
            }
            else
            {
                project.BranchId = dto.BranchId;
            }

            await _db.SaveChangesAsync();

            return await GetProjectByIdAsync(project.Id);
        }

        public async Task<GetProjectDto?> UpdateProjectStatusAsync(Guid projectId, ProjectStatus newStatus)
        {
            var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            if (project is null)
                return null;

            var currentStatus = project.ProjectStatus;

            // Validate business transition rules
            var validTransitions = GetValidTransitions(currentStatus);
            if (!validTransitions.Contains(newStatus))
                return null; // caller treats this as invalid transition

            project.ProjectStatus = newStatus;
            await _db.SaveChangesAsync();

            return await GetProjectByIdAsync(project.Id);
        }

        /// <summary>
        /// Returns the set of states that can be legally transitioned to from the given current state.
        /// </summary>
        private static HashSet<ProjectStatus> GetValidTransitions(ProjectStatus? current) => current switch
        {
            null                       => new HashSet<ProjectStatus> { ProjectStatus.Planning, ProjectStatus.Active, ProjectStatus.Cancelled },
            ProjectStatus.Planning     => new HashSet<ProjectStatus> { ProjectStatus.Active, ProjectStatus.Cancelled },
            ProjectStatus.Active       => new HashSet<ProjectStatus> { ProjectStatus.OnHold, ProjectStatus.Delayed, ProjectStatus.Completed, ProjectStatus.Cancelled },
            ProjectStatus.OnHold       => new HashSet<ProjectStatus> { ProjectStatus.Active, ProjectStatus.Cancelled },
            ProjectStatus.Delayed      => new HashSet<ProjectStatus> { ProjectStatus.Active, ProjectStatus.OnHold, ProjectStatus.Cancelled },
            ProjectStatus.Completed    => new HashSet<ProjectStatus>(),   // terminal
            ProjectStatus.Cancelled    => new HashSet<ProjectStatus>(),   // terminal
            _                          => new HashSet<ProjectStatus>()
        };

        public async Task<GenericResponse> DeleteProjectAsync(Guid projectId)
        {
            var project = await _db.Projects.FindAsync(projectId);
            if (project is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.ProjectNotFound]);

            _db.Projects.Remove(project);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.ProjectDeleteSuccess]);
        }

                // ✅ UPDATED: Filter by branchId
        public async Task<PaginatedList<GetProjectDto>> GetAllProjectsAsync(
            Guid? branchId,
            BaseFilterDto filter,
            ProjectStatus? status = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _db.Projects
                    .Include(p => p.Branch)
                    .AsNoTracking();

                query = ApplyProjectAccessFilter(query);

                // ✅ ADDED: Filter by BranchId if provided
                if (branchId.HasValue && branchId.Value != Guid.Empty)
                {
                    query = query.Where(p => p.BranchId == branchId.Value);
                }

                // Filter by status if provided
                if (status.HasValue)
                {
                    query = query.Where(p => p.ProjectStatus == status.Value);
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

                if (totalCount == 0)
                {
                    return new PaginatedList<GetProjectDto>(
                        new List<GetProjectDto>(),
                        0,
                        filter.PageIndex,
                        filter.PageSize);
                }

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
            catch (Exception)
            {
                return new PaginatedList<GetProjectDto>(
                    new List<GetProjectDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }
        }

        public async Task<GetProjectDto> GetProjectByIdAsync(Guid projectId)
        {
            var filteredQuery = ApplyProjectAccessFilter(_db.Projects.Where(p => p.Id == projectId))
                .Include(p => p.Branch)
                .AsNoTracking();

            var project = await filteredQuery.FirstOrDefaultAsync();
            if (project is null)
                return null!;

            var projectDto = _mapper.Map<GetProjectDto>(project);

            return projectDto;
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
            query = ApplyProjectAccessFilter(query);

            var projects = await query.ToListAsync();
            var projectDtos = _mapper.Map<List<GetProjectDropDownDto>>(projects);

            return projectDtos;
        }

        private IQueryable<Project> ApplyProjectAccessFilter(IQueryable<Project> query)
        {
            var roles = CurrentUser.Roles;

            // SuperAdmin and Admin always see all projects
            var isAdmin = roles.Any(r =>
                string.Equals(r, RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r, RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

            if (isAdmin)
            {
                return query;
            }

            var userId = CurrentUser.Id;
            if (!userId.HasValue)
            {
                return query.Where(_ => false);
            }

            var engineerId = _db.Engineers
                .Where(e => e.ApplicationUserId == userId.Value)
                .Select(e => (Guid?)e.Id)
                .FirstOrDefault();

            if (!engineerId.HasValue)
            {
                return query;
            }

            var allowedProjects = _db.EngineerProjects
                .Where(ep => ep.EngineerId == engineerId.Value)
                .Select(ep => ep.ProjectId);

            // If engineer has no assigned projects, return all
            if (!allowedProjects.Any())
            {
                return query;
            }

            return query.Where(p => allowedProjects.Contains(p.Id));
        }
    }
}