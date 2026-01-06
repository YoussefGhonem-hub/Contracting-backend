using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using static System.Runtime.InteropServices.JavaScript.JSType;
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
            return _mapper.Map<GetEngineerDto>(engineer);
        }

        public async Task<GetEngineerDto> UpdateEngineerAsync(UpdateEngineerDto dto)
        {
            var engineer = await _db.Engineers.FindAsync(dto.Id);
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

            // Change department if provided
            if (dto.ChangeDepartmentId.HasValue)
            {
                engineer.DepartmentId = dto.ChangeDepartmentId.Value;
            }

            await _db.SaveChangesAsync();
            return _mapper.Map<GetEngineerDto>(engineer);
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

        public async Task<PaginatedList<GetEngineerDto>> GetEngineerListAsync(Guid departmentId, BaseFilterDto filter)
        {
            try
            {
                var query = _db.Engineers
                    .Include(x => x.Department)
                    .Include(x => x.ApplicationUser)
                    .Where(e => e.DepartmentId == departmentId)
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
                .Include(e=>e.ApplicationUser)
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

            return dto;
        }

        public async Task<List<GetEngineerDropDownDto>> GetEngineerDropdownAsync(Guid departmentId)
        {
            var engineers = await _db.Engineers.Where(x=>x.DepartmentId == departmentId)
               .Include(e => e.Department) // Ensure Department is eagerly loaded
               .AsNoTracking()
               .ToListAsync();

            return _mapper.Map<List<GetEngineerDropDownDto>>(engineers);
        }

        public async Task<bool> CheckDepartmentHaveManagerAsync(Guid departmentId)
        {
            var hasManager = await (from engineer in _db.Engineers
                                    join userRole in _db.UserRoles on engineer.ApplicationUserId equals userRole.UserId
                                    join role in _db.Roles on userRole.RoleId equals role.Id
                                    where engineer.DepartmentId == departmentId && role.Name == RoleNames.Teamleadengineer
                                    select engineer)
                            .AnyAsync();

            return hasManager;
        }
    }
}
