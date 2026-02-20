using Contracting.Shared.Resources;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
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

            if (dto.hasSpecialFields && dto.SpecialFields.Any())
            {
                await AddProjectSpecialFieldsAsync(project, dto.SpecialFields);
            }

            return await GetProjectByIdAsync(project.Id);
        }

        public async Task<GetProjectDto> UpdateProjectAsync(UpdateProjectDto dto)
        {
            var project = await _db.Projects
                .Include(p => p.ProjectSpecialFields)
                    .ThenInclude(psf => psf.SpecialField)
                .FirstOrDefaultAsync(p => p.Id == dto.Id);

            if (project is null)
                return null!;

            project.nameEn = dto.nameEn;
            project.nameAr = dto.nameAr;
            project.location = dto.location;
            project.Code = dto.Code;
            project.hasSpecialFields = dto.hasSpecialFields;

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

            await ReplaceProjectSpecialFieldsAsync(project, dto.SpecialFields);

            return await GetProjectByIdAsync(project.Id);
        }

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
            CancellationToken cancellationToken = default)
        {
            try
            {
                var query = _db.Projects
                    .Include(p => p.Branch)
                    .Include(p => p.ProjectSpecialFields)
                        .ThenInclude(psf => psf.SpecialField)
                    .AsNoTracking();

                query = ApplyProjectAccessFilter(query);

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
                for (var index = 0; index < projects.Count; index++)
                {
                    var specialFields = projects[index].ProjectSpecialFields
                        .Select(psf => new ProjectSpecialFieldDto
                        {
                            Id = psf.Id,
                            SpecialFieldId = psf.SpecialFieldId,
                            name = psf.SpecialField?.name,
                            fieldType = psf.SpecialField?.fieldType,
                            value = psf.value
                        })
                        .ToList();

                    projectDtos[index].SpecialFields = specialFields;
                    projectDtos[index].hasSpecialFields = specialFields.Any();
                }

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
                .Include(p => p.ProjectSpecialFields).ThenInclude(psf => psf.SpecialField)
                .AsNoTracking();

            var project = await filteredQuery.FirstOrDefaultAsync();
            if (project is null)
                return null!;

            var projectDto = _mapper.Map<GetProjectDto>(project);
            var specialFields = project.ProjectSpecialFields
                .Select(psf => new ProjectSpecialFieldDto
                {
                    Id = psf.Id,
                    SpecialFieldId = psf.SpecialFieldId,
                    name = psf.SpecialField?.name,
                    fieldType = psf.SpecialField?.fieldType,
                    value = psf.value
                })
                .ToList();

            projectDto.SpecialFields = specialFields;
            projectDto.hasSpecialFields = specialFields.Any();

            return projectDto;
        }

        // ✅ UPDATED: Filter by branchId
        public async Task<List<GetProjectDropDownDto>> GetProjectDropdownAsync(Guid? branchId)
        {
            var query = _db.Projects
                .Include(p => p.Branch)
                .Include(p => p.ProjectSpecialFields)
                    .ThenInclude(psf => psf.SpecialField)
                .AsNoTracking();

            // ✅ ADDED: Filter by BranchId if provided
            if (branchId.HasValue && branchId.Value != Guid.Empty)
            {
                query = query.Where(p => p.BranchId == branchId.Value);
            }
            query = ApplyProjectAccessFilter(query);

            var projects = await query.ToListAsync();
            var projectDtos = _mapper.Map<List<GetProjectDropDownDto>>(projects);

            for (var index = 0; index < projects.Count; index++)
            {
                var specialFields = projects[index].ProjectSpecialFields
                    .Select(psf => new ProjectSpecialFieldDto
                    {
                        Id = psf.Id,
                        SpecialFieldId = psf.SpecialFieldId,
                        name = psf.SpecialField?.name,
                        fieldType = psf.SpecialField?.fieldType,
                        value = psf.value
                    })
                    .ToList();

                projectDtos[index].SpecialFields = specialFields;
                projectDtos[index].hasSpecialFields = specialFields.Any();
            }

            return projectDtos;
        }

        public async Task<ProjectSpecialFieldsCheckDto> GetProjectSpecialFieldsAsync(Guid projectId)
        {
            var query = ApplyProjectAccessFilter(_db.Projects.Where(p => p.Id == projectId));

            var project = await query
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (project is null)
                return null!;

            var allProjectSpecialFields = await _db.ProjectSpecialFields
                .IgnoreQueryFilters()
                .Where(psf => psf.ProjectId == projectId)
                .Include(psf => psf.SpecialField)
                .AsNoTracking()
                .ToListAsync();

            var activeProjectSpecialFields = allProjectSpecialFields
                .Where(psf => !psf.IsDeleted && (psf.SpecialField == null || !psf.SpecialField.IsDeleted))
                .ToList();

            var projectSpecialFieldsToReturn = activeProjectSpecialFields.Any()
                ? activeProjectSpecialFields
                : allProjectSpecialFields;

            var specialFields = projectSpecialFieldsToReturn
                .Select(psf => new ProjectSpecialFieldDto
                {
                    Id = psf.Id,
                    SpecialFieldId = psf.SpecialFieldId,
                    name = psf.SpecialField?.name,
                    fieldType = psf.SpecialField?.fieldType,
                    value = psf.value
                })
                .ToList();
            var hasSpecialFields = specialFields.Any();

            return new ProjectSpecialFieldsCheckDto
            {
                ProjectId = project.Id,
                hasSpecialFields = hasSpecialFields,
                SpecialFields = hasSpecialFields
                    ? specialFields
                    : new List<ProjectSpecialFieldDto>()
            };
        }

        private IQueryable<Project> ApplyProjectAccessFilter(IQueryable<Project> query)
        {
            var roles = CurrentUser.Roles;
            var isSiteEngineer = roles.Any(r => string.Equals(r, RoleNames.Siteengineer, StringComparison.OrdinalIgnoreCase));
            if (!isSiteEngineer)
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
                return query.Where(_ => false);
            }

            var allowedProjects = _db.EngineerProjects
                .Where(ep => ep.EngineerId == engineerId.Value)
                .Select(ep => ep.ProjectId);

            return query.Where(p => allowedProjects.Contains(p.Id));
        }

        private async Task AddProjectSpecialFieldsAsync(Project project, List<CreateProjectSpecialFieldDto> fields)
        {
            if (fields is null || fields.Count == 0)
            {
                return;
            }

            var projectFields = new List<ProjectSpecialField>();

            foreach (var field in fields)
            {
                var specialField = new SpecialField
                {
                    name = field.name,
                    fieldType = field.fieldType
                };

                projectFields.Add(new ProjectSpecialField
                {
                    ProjectId = project.Id,
                    SpecialField = specialField,
                    value = field.value
                });
            }

            await _db.ProjectSpecialFields.AddRangeAsync(projectFields);
            await _db.SaveChangesAsync();
        }

        private async Task ReplaceProjectSpecialFieldsAsync(Project project, List<CreateProjectSpecialFieldDto> fields)
        {
            if (!project.hasSpecialFields)
            {
                var toRemove = await _db.ProjectSpecialFields
                    .Include(psf => psf.SpecialField)
                    .Where(psf => psf.ProjectId == project.Id)
                    .ToListAsync();

                if (toRemove.Any())
                {
                    _db.ProjectSpecialFields.RemoveRange(toRemove);
                    var specials = toRemove.Select(e => e.SpecialField).Where(sf => sf != null).ToList();
                    if (specials.Any())
                    {
                        _db.SpecialFields.RemoveRange(specials);
                    }

                    await _db.SaveChangesAsync();
                }

                return;
            }

            if (fields is null || fields.Count == 0)
            {
                return;
            }

            var existing = await _db.ProjectSpecialFields
                .Include(psf => psf.SpecialField)
                .Where(psf => psf.ProjectId == project.Id)
                .ToListAsync();

            if (existing.Any())
            {
                _db.ProjectSpecialFields.RemoveRange(existing);
                var specials = existing.Select(e => e.SpecialField).Where(sf => sf != null).ToList();
                if (specials.Any())
                {
                    _db.SpecialFields.RemoveRange(specials);
                }

                await _db.SaveChangesAsync();
            }

            await AddProjectSpecialFieldsAsync(project, fields);
        }
    }
}