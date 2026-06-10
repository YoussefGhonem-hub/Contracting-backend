using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Common.Enums;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IProjectService
    {
        Task<GetProjectDto> CreateProjectAsync(CreateProjectDto dto);

        // Update
        Task<GetProjectDto> UpdateProjectAsync(UpdateProjectDto dto);

        // Update status with transition validation
        Task<GetProjectDto?> UpdateProjectStatusAsync(Guid projectId, ProjectStatus newStatus);

        // Auto-update statuses based on date rules (called by Hangfire)
        Task ProcessProjectStatusUpdatesAsync(CancellationToken cancellationToken = default);

        // Delete
        Task<GenericResponse> DeleteProjectAsync(Guid projectId);

        // Get All with Pagination
        Task<PaginatedList<GetProjectDto>> GetAllProjectsAsync(Guid? branchId, BaseFilterDto filter, ProjectStatus? status = null, CancellationToken cancellationToken = default);

        // Get By Id
        Task<GetProjectDto> GetProjectByIdAsync(Guid projectId);

        // Dropdown
        Task<List<GetProjectDropDownDto>> GetProjectDropdownAsync(Guid? branchId);

        // All projects by branch (no EngineerProject mapping filter)
        Task<List<GetProjectDropDownDto>> GetProjectsByBranchAsync(Guid branchId, CancellationToken cancellationToken = default);
    }
}
