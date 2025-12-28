using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.EngineerDto;

namespace Contracting.Infrustructure.Inteface
{
    public interface IEngineerService
    {
        // Create a new engineer
        Task<GetEngineerDto> CreateEngineerAsync(CreateEngineerDto dto, Guid UserId);

        // Update an existing engineer
        Task<GetEngineerDto> UpdateEngineerAsync(UpdateEngineerDto dto);

        // Delete an engineer by Id
        Task<bool> DeleteEngineerAsync(Guid engineerId);

        // Get a list of engineers (optionally paginated)
        Task<PaginatedList<GetEngineerDto>> GetEngineerListAsync(Guid DepartmentId, BaseFilterDto filter);

        // Get a single engineer by Id
        Task<GetEngineerDto> GetEngineerByIdAsync(Guid engineerId);

        // Get a dropdown list of engineers (id and name only)
        Task<List<GetEngineerDropDownDto>> GetEngineerDropdownAsync(Guid departmentId);
        Task<bool> CheckDepartmentHaveManagerAsync(Guid departmentId);
    }
}
