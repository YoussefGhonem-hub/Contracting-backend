
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
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

        // Get All with Pagination
        Task<PaginatedList<GetDropDownPriorityDto>> GetAllPrioritiesAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);

        // Dropdown
        Task<List<GetDropDownPriorityDto>> GetPriorityDropdownAsync();
    }
}
