using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IRoleService
    {
        Task<GenericResponse> CreateRoleAsync(CreateRoleDto dto);
        Task<GenericResponse> UpdateRoleAsync(UpdateRoleDto dto);
        Task<GenericResponse> DeleteRoleAsync(Guid roleId);
        Task<PaginatedList<RoleDropDownDto>> GetAllRolesAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);
        Task<List<RoleDropDownDto>> GetRolesDropdownAsync();
    }
}
