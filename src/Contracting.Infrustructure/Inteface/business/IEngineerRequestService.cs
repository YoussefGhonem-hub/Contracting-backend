using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using Contracting.Shared.BusinessDtos.UnifiedRequestDto;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using ErrorOr;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IEngineerRequestService
    {
        // Create
        Task<ErrorOr<GetAllEngineerRequestDto>> CreateEngineerRequestAsync(CreateEngineerRequestDto dto);

        // Create Internal Request (Office Engineer → direct-assigned to another engineer, same branch)
        Task<ErrorOr<GetAllEngineerRequestDto>> CreateInternalRequestAsync(CreateInternalRequestDto dto);

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
        
        // Get all unified requests (EngineerRequest, TransferRequest, LaborAttendance, FinancialClearance)
        // Returns all request types created by or applied to the current engineer
        Task<PaginatedList<GetUnifiedRequestDto>> GetCreatedRequestOrapplaied(
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

        // Confirm delivery date — makes endDate immutable
        Task<ErrorOr<bool>> ConfirmDeliveryDateAsync(Guid requestId);

        // Goods receipt
        Task<ErrorOr<GetAllEngineerRequestDto>> CreateGoodsReceiptAsync(Guid requestId, CreateGoodsReceiptDto dto);
        Task<ErrorOr<List<GetGoodsReceiptDto>>> GetGoodsReceiptsAsync(Guid requestId);
    }
}
