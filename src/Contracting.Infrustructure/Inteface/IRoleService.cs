using Contracting.Shared.MasterDtos.RoleDto;

namespace Contracting.Infrustructure.Inteface
{
    public interface IRoleService
    {
        Task CreateRoleAsync(CreateRoleDto dto);
        Task UpdateRoleAsync(UpdateRoleDto dto);
        Task DeleteRoleAsync(Guid roleId);
        Task<List<RoleDropDownDto>> GetRolesDropdownAsync();
    }
}
