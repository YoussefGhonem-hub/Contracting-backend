using Contracting.Shared.Common;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IRoleService
    {
        Task<GenericResponse> CreateRoleAsync(CreateRoleDto dto);
        Task<GenericResponse> UpdateRoleAsync(UpdateRoleDto dto);
        Task<GenericResponse> DeleteRoleAsync(Guid roleId);
        Task<List<RoleDropDownDto>> GetRolesDropdownAsync();
    }
}
