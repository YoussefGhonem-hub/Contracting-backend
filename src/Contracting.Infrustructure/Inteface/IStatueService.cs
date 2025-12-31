
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IStatueService
    {
        // Create
        Task<GetDropDownStatusDto> CreateStatusAsync(CreateStatusDto dto);

        // Update
        Task<GetDropDownStatusDto> UpdateStatusAsync(UpdateStatusDto dto);

        // Delete
        Task<GenericResponse> DeleteStatusAsync(Guid StatusId);

        // Get All with Pagination
        Task<PaginatedList<GetDropDownStatusDto>> GetAllStatusesAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);

        // Dropdown
        Task<List<GetDropDownStatusDto>> GetStatusDropdownAsync();
    }
}
