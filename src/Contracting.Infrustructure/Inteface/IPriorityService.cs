
using Contracting.Shared.MasterDtos.PriorityDto;

namespace Contracting.Infrustructure.Inteface
{
    public interface IPriorityService
    {
        // Create
        Task<GetDropDownPriorityDto> CreatePriorityAsync(CreatePriorityDto dto);

        // Update
        Task<GetDropDownPriorityDto> UpdatePriorityAsync(UpdatePriorityDto dto);

        // Delete
        Task<bool> DeletePriorityAsync(Guid priorityId);

        // Dropdown
        Task<List<GetDropDownPriorityDto>> GetPriorityDropdownAsync();
    }
}
