using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Contracting.Shared.Dtos.MasterDtos.BranchDto;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;

namespace Contracting.Infrustructure.Features
{
    public class EngineerService : IEngineerService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStringLocalizer<SharedResources> _localizer;


        public EngineerService(ApplicationDbContext db, IMapper mapper, IStringLocalizer<SharedResources> localizer)
        {
            _db = db;
            _mapper = mapper;
            _localizer = localizer;
        }

        public async Task<GetEngineerDto> CreateEngineerAsync(CreateEngineerDto dto, Guid UserId)
        {
            var engineer = _mapper.Map<Engineer>(dto);
            engineer.ApplicationUserId = UserId;
            if (engineer.DepartmentId == Guid.Empty || engineer.DepartmentId == null)
            {
                engineer.DepartmentId = null;
            }

            await _db.Engineers.AddAsync(engineer);
            await _db.SaveChangesAsync();

            await ReplaceEngineerProjectsAsync(engineer.Id, dto.Projects);

            return _mapper.Map<GetEngineerDto>(engineer);
        }

        public async Task<GetEngineerDto> UpdateEngineerAsync(UpdateEngineerDto dto)
        {
            var engineer = await _db.Engineers
                .Include(e => e.EngineerProjects)
                .FirstOrDefaultAsync(e => e.Id == dto.Id);
            if (engineer is null)
                return null!;

            // Map updated fields
            engineer.nameEn = dto.nameEn;
            engineer.nameAr = dto.nameAr;
            engineer.address = dto.address;
            engineer.passportNumber = dto.passportNumber;
            engineer.nationalId = dto.nationalId;
            engineer.position = dto.position;
            engineer.phoneNumber = dto.phoneNumber;
            engineer.yearExperience = dto.yearExperience;
            engineer.Email = dto.Email;
            engineer.EffectiveDate = dto.EffectiveDate;

            // Change department if provided
            if (dto.ChangeDepartmentId.HasValue)
            {
                engineer.DepartmentId = dto.ChangeDepartmentId.Value;
            }

            await _db.SaveChangesAsync();

            // Merge simple projectIds (Guids) into the Projects list so both formats work.
            // ProjectIds takes precedence: if provided it replaces the Projects list entirely.
            var projectsToAssign = dto.ProjectIds != null && dto.ProjectIds.Count > 0
                ? dto.ProjectIds.Distinct().Select(id => new ProjectAssignDto
                  {
                      ProjectId = id,
                      IsProjectManager = dto.Projects?.FirstOrDefault(p => p.ProjectId == id)?.IsProjectManager ?? false
                  }).ToList()
                : dto.Projects ?? new();

            await ReplaceEngineerProjectsAsync(engineer.Id, projectsToAssign);
            return await GetEngineerByIdAsync(engineer.Id);
        }
        public async Task UpdateUserRolesAsync(Guid userId, List<Guid> roleIds)
        {
            // Remove existing roles
            var existingRoles = _db.UserRoles.Where(ur => ur.UserId == userId);
            _db.UserRoles.RemoveRange(existingRoles);

            // Add new roles
            var userRoles = roleIds.Select(roleId => new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = roleId
            }).ToList();

            await _db.UserRoles.AddRangeAsync(userRoles);
            await _db.SaveChangesAsync();
        }

        public async Task<ApplicationUser> UpdateUserAsync(Guid userId, UpdateEngineerDto dto)
        {
            var user = await _db.Users.FindAsync(userId);
            if (user is null)
                return null;

            user.UserName = dto.Email;
            user.Email = dto.Email;
            user.FullName = dto.nameEn;
            user.PhoneNumber = dto.phoneNumber;
            user.IsActive = true;

            _db.Users.Update(user);
            await _db.SaveChangesAsync();

            return user;
        }

        public async Task<GenericResponse> DeleteEngineerAsync(Guid engineerId)
        {
            var engineer = await _db.Engineers.FindAsync(engineerId);
            if (engineer is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.EngineerNotFound]);

            _db.Engineers.Remove(engineer);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.EngineerDeleteSuccess]);
        }

        public async Task<PaginatedList<GetEngineerDto>> GetEngineerListAsync(Guid departmentId, BaseFilterDto filter, string? name = null, string? email = null)
        {
            try
            {
                var query = _db.Engineers
                    .Include(x => x.Department)
                    .ThenInclude(x => x.Branch)
                    .Include(x => x.ApplicationUser)
                    .Include(x => x.EngineerProjects)
                        .ThenInclude(ep => ep.Project)
                            .ThenInclude(p => p.Branch)
                    .Include(x => x.EngineerProjects)
                        .ThenInclude(ep => ep.Features)
                    .Where(e => e.DepartmentId == departmentId)
                    .AsNoTracking();

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(e => (e.nameEn != null && e.nameEn.Contains(name))
                                           || (e.nameAr != null && e.nameAr.Contains(name)));

                if (!string.IsNullOrWhiteSpace(email))
                    query = query.Where(e => e.Email != null && e.Email.Contains(email));

                if (string.IsNullOrWhiteSpace(filter.Sort))
                {
                    query = query.OrderBy(e => e.CreatedDate);
                }
                else
                {
                    query = query.OrderByDynamic(filter.Sort, filter.Descending);
                }

                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                {
                    return new PaginatedList<GetEngineerDto>(
                        new List<GetEngineerDto>(),
                        0,
                        filter.PageIndex,
                        filter.PageSize);
                }

                var engineers = await query
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Get all user IDs
                var userIds = engineers.Select(e => e.ApplicationUserId).ToList();

                // Get all roles for these users in one query
                var userRolesDict = await _db.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId))
                    .Join(_db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, RoleId = r.Id, RoleName = r.Name })
                    .GroupBy(x => x.UserId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(x => new RoleDropDownDto { Id = x.RoleId, Name = x.RoleName }).ToList());

                // Map to DTOs and assign roles
                var engineerDtos = engineers.Select(engineer =>
                {
                    var dto = _mapper.Map<GetEngineerDto>(engineer);
                    dto.Roles = userRolesDict.ContainsKey(engineer.ApplicationUserId)
                        ? userRolesDict[engineer.ApplicationUserId]
                        : new List<RoleDropDownDto>();
                    dto.Projects = MapEngineerProjects(engineer.EngineerProjects);
                    return dto;
                }).ToList();

                return new PaginatedList<GetEngineerDto>(
                    engineerDtos,
                    totalCount,
                    filter.PageIndex,
                    filter.PageSize);
            }
            catch (Exception)
            {
                // Return empty list on error
                return new PaginatedList<GetEngineerDto>(
                    new List<GetEngineerDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }
        }

        public async Task<PaginatedList<GetEngineerDto>> GetAllEngineersAsync(BaseFilterDto filter)
        {
            try
            {
                var query = _db.Engineers
                    .Include(x => x.Department)
                    .ThenInclude(x=>x.Branch)
                    .Include(x => x.ApplicationUser)
                    .Include(x => x.EngineerProjects)
                        .ThenInclude(ep => ep.Project)
                            .ThenInclude(p => p.Branch)
                    .Include(x => x.EngineerProjects)
                        .ThenInclude(ep => ep.Features)
                    .AsNoTracking();

                if (string.IsNullOrWhiteSpace(filter.Sort))
                {
                    query = query.OrderBy(e => e.CreatedDate);
                }
                else
                {
                    query = query.OrderByDynamic(filter.Sort, filter.Descending);
                }

                var totalCount = await query.CountAsync();

                if (totalCount == 0)
                {
                    return new PaginatedList<GetEngineerDto>(
                        new List<GetEngineerDto>(),
                        0,
                        filter.PageIndex,
                        filter.PageSize);
                }

                var engineers = await query
                    .Skip((filter.PageIndex - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .ToListAsync();

                // Get all user IDs
                var userIds = engineers.Select(e => e.ApplicationUserId).ToList();

                // Get all roles for these users in one query
                var userRolesDict = await _db.UserRoles
                    .Where(ur => userIds.Contains(ur.UserId))
                    .Join(_db.Roles,
                        ur => ur.RoleId,
                        r => r.Id,
                        (ur, r) => new { ur.UserId, RoleId = r.Id, RoleName = r.Name })
                    .GroupBy(x => x.UserId)
                    .ToDictionaryAsync(
                        g => g.Key,
                        g => g.Select(x => new RoleDropDownDto { Id = x.RoleId, Name = x.RoleName }).ToList());

                // Map to DTOs and assign roles
                var engineerDtos = engineers.Select(engineer =>
                {
                    var dto = _mapper.Map<GetEngineerDto>(engineer);
                    dto.Roles = userRolesDict.ContainsKey(engineer.ApplicationUserId)
                        ? userRolesDict[engineer.ApplicationUserId]
                        : new List<RoleDropDownDto>();
                    dto.Projects = MapEngineerProjects(engineer.EngineerProjects);
                    return dto;
                }).ToList();

                return new PaginatedList<GetEngineerDto>(
                    engineerDtos,
                    totalCount,
                    filter.PageIndex,
                    filter.PageSize);
            }
            catch (Exception)
            {
                // Return empty list on error
                return new PaginatedList<GetEngineerDto>(
                    new List<GetEngineerDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }
        }

        public async Task<GetEngineerDto> GetEngineerByIdAsync(Guid engineerId)
        {
            var engineer = await _db.Engineers
                .Include(e => e.Department)
                .ThenInclude(e => e.Branch)
                .Include(e=>e.ApplicationUser)
                .Include(e => e.EngineerProjects)
                    .ThenInclude(ep => ep.Project)
                        .ThenInclude(p => p.Branch)
                .Include(e => e.EngineerProjects)
                    .ThenInclude(ep => ep.Features)
                .Include(e => e.EngineerDepartments)
                    .ThenInclude(ed => ed.Department)
                        .ThenInclude(d => d.Branch)
                .Include(e => e.EngineerDepartments)
                    .ThenInclude(ed => ed.Role)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == engineerId);

            if (engineer is null)
                return null!;

            // Map to DTO
            var dto = _mapper.Map<GetEngineerDto>(engineer);
            dto.ApplicationUserId = engineer.ApplicationUserId;

            // Get roles for this engineer
            var roles = await _db.UserRoles
                .Where(ur => ur.UserId == engineer.ApplicationUserId)
                .Join(_db.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => new RoleDropDownDto { Id = r.Id, Name = r.Name })
                .ToListAsync();

            dto.Roles = roles;
            dto.Projects = MapEngineerProjects(engineer.EngineerProjects);

            // Populate department roles
            dto.DepartmentRoles = engineer.EngineerDepartments.Select(ed => new EngineerDepartmentRoleDto
            {
                Id = ed.Id,
                DepartmentId = ed.DepartmentId,
                Department = _mapper.Map<Contracting.Shared.Dtos.MasterDtos.DepartmentDtos.GetDepartmentDto>(ed.Department),
                RoleId = ed.RoleId,
                Role = new RoleDropDownDto { Id = ed.Role!.Id, Name = ed.Role.Name! }
            }).ToList();

            return dto;
        }

        public async Task<List<GetEngineerDropDownDto>> GetEngineerDropdownAsync(Guid departmentId)
        {
            var engineers = await _db.Engineers.Where(x=>x.DepartmentId == departmentId)
               .Include(e => e.Department)
                   .ThenInclude(e => e.Branch) // Ensure Department.Branch is eagerly loaded
               .AsNoTracking()
               .ToListAsync();

            return _mapper.Map<List<GetEngineerDropDownDto>>(engineers);
        }

        public async Task<List<GetEngineerProjectDto>> GetEngineerProjectsAsync(Guid engineerId, Guid? branchId = null)
        {
            var isSuperAdmin = CurrentUser.Roles.Any(r =>
                r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));

            if (isSuperAdmin)
            {
                var projectsQuery = _db.Projects
                    .Include(p => p.Branch)
                    .AsNoTracking()
                    .AsQueryable();

                if (branchId.HasValue)
                    projectsQuery = projectsQuery.Where(p => p.BranchId == branchId.Value);

                var projects = await projectsQuery.ToListAsync();

                return projects.Select(p => new GetEngineerProjectDto
                {
                    ProjectId = p.Id,
                    nameEn = p.nameEn,
                    nameAr = p.nameAr,
                    location = p.location,
                    Code = p.Code,
                    imageUrl = p.imageUrl,
                    BranchId = p.BranchId,
                    Branch = p.Branch == null ? null : new GetBranchDto
                    {
                        Id = p.Branch.Id,
                        nameEn = p.Branch.nameEn,
                        nameAr = p.Branch.nameAr,
                        address = p.Branch.address,
                        location = p.Branch.location,
                        currency = p.Branch.currency
                    },
                    IsProjectManager = false,
                    ProjectStatus = p.ProjectStatus,
                    Features = new List<string>()
                }).ToList();
            }

            var engineerExists = await _db.Engineers
                .IgnoreQueryFilters()
                .AsNoTracking()
                .AnyAsync(e => e.Id == engineerId);

            if (!engineerExists)
                return new List<GetEngineerProjectDto>();

            var engineerProjectsQuery = _db.EngineerProjects
                .Where(ep => ep.EngineerId == engineerId)
                .Include(ep => ep.Project)
                    .ThenInclude(p => p.Branch)
                .Include(ep => ep.Features)
                .AsNoTracking()
                .AsQueryable();

            if (branchId.HasValue)
                engineerProjectsQuery = engineerProjectsQuery.Where(ep => ep.Project!.BranchId == branchId.Value);

            var engineerProjects = await engineerProjectsQuery.ToListAsync();

            return MapEngineerProjects(engineerProjects);
        }

        private async Task ReplaceEngineerProjectsAsync(Guid engineerId, List<ProjectAssignDto> projects)
        {
            var existing = await _db.EngineerProjects
                .Where(ep => ep.EngineerId == engineerId)
                .ToListAsync();

            if (existing.Any())
                _db.EngineerProjects.RemoveRange(existing);

            if (projects is null || projects.Count == 0)
            {
                await _db.SaveChangesAsync();
                return;
            }

            var newLinks = projects
                .GroupBy(p => p.ProjectId)
                .Select(g => g.First())
                .Select(p => new EngineerProject
                {
                    EngineerId = engineerId,
                    ProjectId = p.ProjectId,
                    IsProjectManager = p.IsProjectManager,
                    Features = (p.Features ?? new List<string>())
                        .Distinct()
                        .Select(f => new EngineerProjectFeature { Feature = f })
                        .ToList()
                });

            await _db.EngineerProjects.AddRangeAsync(newLinks);
            await _db.SaveChangesAsync();
        }

        public async Task<ErrorOr<GetEngineerProjectDto>> UpdateEngineerProjectFeaturesAsync(
            Guid engineerId, Guid projectId, List<string> features)
        {
            var link = await _db.EngineerProjects
                .Include(ep => ep.Features)
                .Include(ep => ep.Project).ThenInclude(p => p!.Branch)
                .FirstOrDefaultAsync(ep => ep.EngineerId == engineerId && ep.ProjectId == projectId);

            if (link is null)
                return ErrorOr.Error.NotFound("EngineerProject.NotFound", "Engineer is not assigned to this project.");

            _db.EngineerProjectFeatures.RemoveRange(link.Features);

            var newFeatures = (features ?? new List<string>())
                .Distinct()
                .Select(f => new EngineerProjectFeature { EngineerProjectId = link.Id, Feature = f })
                .ToList();

            await _db.EngineerProjectFeatures.AddRangeAsync(newFeatures);
            await _db.SaveChangesAsync();

            link.Features = newFeatures;
            return MapEngineerProject(link);
        }

        private static List<GetEngineerProjectDto> MapEngineerProjects(IEnumerable<EngineerProject> engineerProjects)
            => engineerProjects.Select(MapEngineerProject).ToList();

        private static GetEngineerProjectDto MapEngineerProject(EngineerProject ep) => new()
        {
            ProjectId = ep.ProjectId,
            nameEn = ep.Project?.nameEn,
            nameAr = ep.Project?.nameAr,
            location = ep.Project?.location,
            Code = ep.Project?.Code,
            imageUrl = ep.Project?.imageUrl,
            BranchId = ep.Project?.BranchId,
            Branch = ep.Project?.Branch == null ? null : new GetBranchDto
            {
                Id = ep.Project.Branch.Id,
                nameEn = ep.Project.Branch.nameEn,
                nameAr = ep.Project.Branch.nameAr,
                address = ep.Project.Branch.address,
                location = ep.Project.Branch.location,
                currency = ep.Project.Branch.currency
            },
            IsProjectManager = ep.IsProjectManager,
            ProjectStatus = ep.Project?.ProjectStatus,
            Features = ep.Features?.Select(f => f.Feature).ToList() ?? new List<string>()
        };

        public async Task<bool> CheckDepartmentHaveManagerAsync(Guid departmentId, Guid? excludeEngineerId = null)
        {
            var query = from engineer in _db.Engineers
                        join userRole in _db.UserRoles on engineer.ApplicationUserId equals userRole.UserId
                        join role in _db.Roles on userRole.RoleId equals role.Id
                        where engineer.DepartmentId == departmentId && role.Name == RoleNames.Teamleadengineer
                        select engineer;

            // Exclude the current engineer being updated
            if (excludeEngineerId.HasValue)
            {
                query = query.Where(e => e.Id != excludeEngineerId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task ReplaceEngineerDepartmentsAsync(Guid engineerId, List<DepartmentRoleDto> departmentRoles)
        {
            // Hard-delete existing records to avoid unique constraint violation
            // (soft delete via ApplyAuditing keeps the row, conflicting with the unique index on EngineerId+DepartmentId)
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM master.EngineerDepartments WHERE EngineerId = {engineerId}");

            if (departmentRoles is null || departmentRoles.Count == 0)
                return;

            var newLinks = departmentRoles.Select(dr => new EngineerDepartment
            {
                EngineerId = engineerId,
                DepartmentId = dr.DepartmentId,
                RoleId = dr.RoleId
            });

            await _db.EngineerDepartments.AddRangeAsync(newLinks);
            await _db.SaveChangesAsync();
        }

        public async Task<List<EngineerDepartmentRoleDto>> GetEngineerDepartmentsAsync(Guid engineerId)
        {
            var engineerDepartments = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineerId)
                .Include(ed => ed.Department)
                    .ThenInclude(d => d.Branch)
                .Include(ed => ed.Role)
                .AsNoTracking()
                .ToListAsync();

            return engineerDepartments.Select(ed => new EngineerDepartmentRoleDto
            {
                Id = ed.Id,
                DepartmentId = ed.DepartmentId,
                Department = _mapper.Map<Contracting.Shared.Dtos.MasterDtos.DepartmentDtos.GetDepartmentDto>(ed.Department),
                RoleId = ed.RoleId,
                Role = new Contracting.Shared.Dtos.MasterDtos.RoleDtos.RoleDropDownDto
                {
                    Id = ed.Role!.Id,
                    Name = ed.Role.Name!
                }
            }).ToList();
        }

        public async Task SwitchActiveDepartmentAsync(Guid engineerId, Guid departmentId)
        {
            var engineer = await _db.Engineers.FindAsync(engineerId);
            if (engineer is null) return;

            engineer.DepartmentId = departmentId;
            await _db.SaveChangesAsync();
        }

        public async Task<GenericResponse> DeleteEngineerDepartmentAsync(Guid engineerId, Guid departmentId)
        {
            var engineerDepartment = await _db.EngineerDepartments
                .FirstOrDefaultAsync(ed => ed.EngineerId == engineerId && ed.DepartmentId == departmentId);

            if (engineerDepartment is null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.DepartmentNotFound]);

            // Hard-delete to avoid unique constraint conflict on future re-assignment
            await _db.Database.ExecuteSqlInterpolatedAsync(
                $"DELETE FROM master.EngineerDepartments WHERE Id = {engineerDepartment.Id}");

            return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.DepartmentRemoveSuccess]);
        }
    }
}
