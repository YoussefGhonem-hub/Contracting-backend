using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
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

        public async Task<GenericResponse> CreateRoleAsync(CreateRoleDto dto)
        {
            var role = new ApplicationRole
            {
                Name = dto.Name,
                DisplayName = dto.DisplayName
            };

            var result = await _roleManager.CreateAsync(role);
            return result.Succeeded
                ? GenericResponse.SuccessResult("Role created successfully")
                : GenericResponse.FailureResult("Failed to create role", result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<GenericResponse> UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _roleManager.FindByIdAsync(dto.Id.ToString());
            if (role == null)
                return GenericResponse.FailureResult("Role not found");

            role.Name = dto.Name;
            role.DisplayName = dto.DisplayName;

            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded
                ? GenericResponse.SuccessResult("Role updated successfully")
                : GenericResponse.FailureResult("Failed to update role", result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<GenericResponse> DeleteRoleAsync(Guid roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
                return GenericResponse.FailureResult("Role not found");

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded
                ? GenericResponse.SuccessResult("Role deleted successfully")
                : GenericResponse.FailureResult("Failed to delete role", result.Errors.Select(e => e.Description).ToArray());
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
