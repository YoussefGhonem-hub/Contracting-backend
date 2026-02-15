using Contracting.Domain.Entities;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IEngineerService
    {
        // Create a new engineer
        Task<GetEngineerDto> CreateEngineerAsync(CreateEngineerDto dto, Guid UserId);

        // Update an existing engineer
        Task<GetEngineerDto> UpdateEngineerAsync(UpdateEngineerDto dto);

        // Delete an engineer by Id
        Task<GenericResponse> DeleteEngineerAsync(Guid engineerId);

        // Get a list of engineers (optionally paginated)
        Task<PaginatedList<GetEngineerDto>> GetEngineerListAsync(Guid DepartmentId, BaseFilterDto filter);

        // Get all engineers (optionally paginated)
        Task<PaginatedList<GetEngineerDto>> GetAllEngineersAsync(BaseFilterDto filter);

        // Get a single engineer by Id
        Task<GetEngineerDto> GetEngineerByIdAsync(Guid engineerId);

        // Get a dropdown list of engineers (id and name only)
        Task<List<GetEngineerDropDownDto>> GetEngineerDropdownAsync(Guid departmentId);
        Task<List<GetProjectDropDownDto>> GetEngineerProjectsAsync(Guid engineerId);
        Task<bool> CheckDepartmentHaveManagerAsync(Guid departmentId, Guid? excludeEngineerId = null);
        Task UpdateUserRolesAsync(Guid userId, List<Guid> roleIds);
        Task<ApplicationUser> UpdateUserAsync(Guid userId, UpdateEngineerDto dto);

    }
}
