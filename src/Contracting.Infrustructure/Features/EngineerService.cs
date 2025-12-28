using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.EngineerDto;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Contracting.Infrustructure.Features
{
    public class EngineerService : IEngineerService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;


        public EngineerService(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
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
        public async Task UpdateUserRolesAsync(Guid userId, List<string> roles)
        {
            // Remove existing roles
            var existingRoles = _db.UserRoles.Where(ur => ur.UserId == userId);
            _db.UserRoles.RemoveRange(existingRoles);

            // Add new roles
            var userRoles = roles.Select(roleName => new IdentityUserRole<Guid>
            {
                UserId = userId,
                RoleId = _db.Roles.First(r => r.Name == roleName).Id
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



        public async Task<bool> DeleteEngineerAsync(Guid engineerId)
        {
            var engineer = await _db.Engineers.FindAsync(engineerId);
            if (engineer is null)
                return false;

            _db.Engineers.Remove(engineer);
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<PaginatedList<GetEngineerDto>> GetEngineerListAsync(Guid departmentId, BaseFilterDto filter)
        {
            var query = _db.Engineers.Include(x=>x.Department).AsNoTracking();            
            query = query.Where(e => e.DepartmentId == departmentId);           
            return await query.PaginateAsync<Engineer, GetEngineerDto>(filter.PageIndex, filter.PageSize);
        }

        public async Task<GetEngineerDto> GetEngineerByIdAsync(Guid engineerId)
        {
            var engineer = await _db.Engineers
                .Include(e => e.Department)
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == engineerId);

            return engineer is null ? null! : _mapper.Map<GetEngineerDto>(engineer);
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
                                    where engineer.DepartmentId == departmentId && role.Name == "team-lead"
                                    select engineer)
                            .AnyAsync();

            return hasManager;
        }
    }
}
