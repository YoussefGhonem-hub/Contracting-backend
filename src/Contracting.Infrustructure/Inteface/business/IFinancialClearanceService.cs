using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using Contracting.Shared.Common;
using ErrorOr;

namespace Contracting.Infrustructure.Inteface.business
{
    public interface IFinancialClearanceService
    {
        Task<ErrorOr<GetFinancialClearanceDto>> CreateAsync(CreateFinancialClearanceDto dto);
        Task<ErrorOr<GetFinancialClearanceDto>> UpdateAsync(UpdateFinancialClearanceDto dto);
        Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id);
        Task<ErrorOr<GetFinancialClearanceDto>> GetByIdAsync(Guid id);
        /// <summary>Anonymous printable-report read — skips the CurrentUser visibility filter.</summary>
        Task<ErrorOr<GetFinancialClearanceDto>> GetByIdForPublicReportAsync(Guid id);
        Task<PaginatedList<GetFinancialClearanceDto>> GetAllAsync(FinancialClearanceFilterDto filter);
        Task<ErrorOr<GetFinancialClearanceDto>> TakeActionAsync(Guid id, FinancialClearanceActionDto dto);
        Task<ErrorOr<GetFinancialClearanceDto>> ReassignAsync(Guid id, ReassignFinancialClearanceDto dto);
    }
}
