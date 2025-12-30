using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using Contracting.Shared.MasterDtos.RoleDto;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Contracting.Infrustructure.Features
{
    public class RoleService : IRoleService
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public RoleService(RoleManager<ApplicationRole> roleManager, IStringLocalizer<SharedResources> localizer)
        {
            _roleManager = roleManager;
            _localizer = localizer;
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
                ? GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.RoleCreateSuccess])
                : GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CreateSuccess], result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<GenericResponse> UpdateRoleAsync(UpdateRoleDto dto)
        {
            var role = await _roleManager.FindByIdAsync(dto.Id.ToString());
            if (role == null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RoleNotFound]);

            role.Name = dto.Name;
            role.DisplayName = dto.DisplayName;

            var result = await _roleManager.UpdateAsync(role);
            return result.Succeeded
                ? GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.RoleUpdateSuccess])
                : GenericResponse.FailureResult(_localizer[SharedResourcesKeys.UpdateSuccess], result.Errors.Select(e => e.Description).ToArray());
        }

        public async Task<GenericResponse> DeleteRoleAsync(Guid roleId)
        {
            var role = await _roleManager.FindByIdAsync(roleId.ToString());
            if (role == null)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RoleNotFound]);

            var result = await _roleManager.DeleteAsync(role);
            return result.Succeeded
                ? GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.RoleDeleteSuccess])
                : GenericResponse.FailureResult(_localizer[SharedResourcesKeys.DeleteFailed], result.Errors.Select(e => e.Description).ToArray());
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
