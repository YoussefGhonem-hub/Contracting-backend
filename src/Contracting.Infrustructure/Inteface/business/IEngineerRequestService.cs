using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using ErrorOr;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerRequestService
    {
        // Create
        Task<GetAllEngineerRequestDto> CreateEngineerRequestAsync(CreateEngineerRequestDto dto);

        // Update (if not yet actioned, or if in Missing Information status)
        Task<ErrorOr<GetAllEngineerRequestDto>> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto);

        // Delete
        Task<GenericResponse> DeleteEngineerRequestAsync(Guid requestId);

        // Get All filtered by DepartmentId (for managers)
        Task<PaginatedList<GetAllEngineerRequestDto>> GetAllEngineerRequestsByDepartmentAsync(
            Guid departmentId,
            BaseFilterDto filter,
            CancellationToken cancellationToken = default);
        Task<PaginatedList<GetAllEngineerRequestDto>> FilterEngineerRequestsAsync(
            EngineerRequestFilterDto filter,
            CancellationToken cancellationToken = default);
        Task<PaginatedList<GetAllEngineerRequestDto>> GetCreatedRequestOrapplaied(
            EngineerRequestParticipationFilterDto filter,
            CancellationToken cancellationToken = default);

        // Get requests by status for current engineer (assigned to or created by)
        Task<PaginatedList<GetAllEngineerRequestDto>> GetRequestsByStatusForEngineerAsync(
            GetRequestsByStatusFilterDto filter,
            CancellationToken cancellationToken = default);

        // Get request by ID
        Task<GetAllEngineerRequestDto> GetEngineerRequestByIdAsync(Guid requestId);

        // Check if engineer is manager of department
        Task<bool> IsEngineerManagerOfDepartmentAsync(Guid engineerId, Guid departmentId);

        // Action on request (approve/reject) - creates entry in action table
        Task<GenericResponse> TakeActionOnRequestAsync(Guid requestId, Guid currentUserId, TakeActionRequestDto actionDto);
        Task<GenericResponse> ReassignEngineerRequestAsync(Guid requestId, Guid currentUserId, ReassignEngineerRequestDto dto);
        
        // Get activities for a request
        Task<List<GetEngineerRequestActiviteDto>> GetRequestActivitiesAsync(Guid requestId);

        // Get count of engineer requests by status
        Task<List<GetEngineerRequestCountByStatusDto>> GetEngineerRequestCountByStatusAsync(Guid engineerId);
        Task<bool> DepartmentHasTeamLeadAsync(Guid departmentId);

        // Scheduled automation
        Task ProcessScheduledStatusUpdatesAsync(CancellationToken cancellationToken = default);

    }
}
