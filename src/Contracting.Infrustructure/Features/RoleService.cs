using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Infrustructure.Extensions;

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

        public async Task<PaginatedList<RoleDropDownDto>> GetAllRolesAsync(BaseFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _roleManager.Roles.AsQueryable();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderBy(r => r.Name);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = query.Count();

            var roles = query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(r => new RoleDropDownDto
                {
                    Id = r.Id,
                    Name = r.Name
                })
                .ToList();

            return new PaginatedList<RoleDropDownDto>(
                roles,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
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
