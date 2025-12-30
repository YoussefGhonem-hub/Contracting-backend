using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerRequestService
    {
        // Create
        Task<GetAllEngineerRequestDto> CreateEngineerRequestAsync(CreateEngineerRequestDto dto);

        // Update (if not yet actioned)
        Task<GetAllEngineerRequestDto> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto);

        // Delete
        Task<GenericResponse> DeleteEngineerRequestAsync(Guid requestId);

        // Get All filtered by DepartmentId (for managers)
        Task<PaginatedList<GetAllEngineerRequestDto>> GetAllEngineerRequestsByDepartmentAsync(
            Guid departmentId,
            BaseFilterDto filter,
            CancellationToken cancellationToken = default);
        Task<PaginatedList<GetAllEngineerRequestDto>> GetCreatedRequestOrapplaied(
            BaseFilterDto filter,
            CancellationToken cancellationToken = default);

        // Get request by ID
        Task<GetAllEngineerRequestDto> GetEngineerRequestByIdAsync(Guid requestId);

        // Check if engineer is manager of department
        Task<bool> IsEngineerManagerOfDepartmentAsync(Guid engineerId, Guid departmentId);

        // Action on request (approve/reject) - creates entry in action table
        Task<GenericResponse> TakeActionOnRequestAsync(Guid requestId, Guid currentUserId, TakeActionRequestDto actionDto);
        
        // Get activities for a request
        Task<List<GetEngineerRequestActiviteDto>> GetRequestActivitiesAsync(Guid requestId);
    }
}
