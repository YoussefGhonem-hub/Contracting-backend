using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using Contracting.Shared.Common;
using ErrorOr;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface ILaborAttendanceService
    {
        Task<ErrorOr<GetLaborAttendanceRequestDto>> CreateAsync(CreateLaborAttendanceRequestDto dto);
        Task<ErrorOr<GetLaborAttendanceRequestDto>> UpdateAsync(UpdateLaborAttendanceRequestDto dto);
        Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id);
        Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdAsync(Guid id);
        /// <summary>Anonymous printable-report read — skips the CurrentUser visibility filter.</summary>
        Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdForPublicReportAsync(Guid id);
        Task<PaginatedList<GetLaborAttendanceRequestDto>> GetAllAsync(LaborAttendanceFilterDto filter);
        Task<ErrorOr<GetLaborAttendanceRequestDto>> TakeActionAsync(Guid id, LaborAttendanceActionDto dto);
        Task<ErrorOr<GetLaborAttendanceRequestDto>> ReassignAsync(Guid id, ReassignLaborAttendanceDto dto);
    }
}
