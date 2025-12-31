
using Contracting.Shared.Common;
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

        // Dropdown
        Task<List<GetDropDownStatusDto>> GetStatusDropdownAsync();
    }
}
