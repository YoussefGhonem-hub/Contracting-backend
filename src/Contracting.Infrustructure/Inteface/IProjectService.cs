using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.ProjectDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IProjectService
    {
        Task<GetProjectDto> CreateProjectAsync(CreateProjectDto dto);

        // Update
        Task<GetProjectDto> UpdateProjectAsync(UpdateProjectDto dto);

        // Delete
        Task<GenericResponse> DeleteProjectAsync(Guid projectId);

        // Get All with Pagination
        Task<PaginatedList<GetProjectDto>> GetAllProjectsAsync(Guid? branchId, BaseFilterDto filter, CancellationToken cancellationToken = default);

        // Get By Id
        Task<GetProjectDto> GetProjectByIdAsync(Guid projectId);

        // Dropdown
        Task<List<GetProjectDropDownDto>> GetProjectDropdownAsync(Guid? branchId);
    }
}
