
using Contracting.Shared.Common;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;

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
