
using Contracting.Shared.Common;
using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.StatusDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IPriorityService
    {
        // Create
        Task<GetDropDownPriorityDto> CreatePriorityAsync(CreatePriorityDto dto);

        // Update
        Task<GetDropDownPriorityDto> UpdatePriorityAsync(UpdatePriorityDto dto);

        // Delete
        Task<GenericResponse> DeletePriorityAsync(Guid priorityId);

        // Dropdown
        Task<List<GetDropDownPriorityDto>> GetPriorityDropdownAsync();
    }
}
