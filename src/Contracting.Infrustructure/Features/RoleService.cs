using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.RoleDto;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Infrustructure.Features
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public RoleService(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new ApplicationRole
            {
                Name = dto.Name,
                DisplayName = dto.DisplayName
            };

            await _roleManager.CreateAsync(role);
        }

        public async Task UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _roleManager.FindByIdAsync(dto.Id.ToString());
            if (role == null) throw new Exception("Role not found");

            role.Name = dto.Name;
            role.DisplayName = dto.DisplayName;

            await _roleManager.UpdateAsync(role);
        }

        public async Task DeleteRoleAsync(Guid roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null) throw new Exception("Role not found");

            await _roleManager.DeleteAsync(role);
        }

        public async Task<List<RoleDropDownDto>> GetRolesDropdownAsync()
        {
            return _roleManager.Roles
                .Select(r => new RoleDropDownDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToList();
        }
    }
}
