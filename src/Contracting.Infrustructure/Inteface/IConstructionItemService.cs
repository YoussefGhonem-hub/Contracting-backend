using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using Contracting.Infrustructure.Extensions.Helpers;

namespace Contracting.Infrustructure.Inteface
{
    public interface IConstructionItemService
    {
        Task<GetConstructionItemDto> CreateAsync(CreateConstructionItemDto dto);
        Task<GetConstructionItemDto> UpdateAsync(UpdateConstructionItemDto dto);
        Task<GenericResponse> DeleteAsync(Guid id);
        Task<GetConstructionItemDto> GetByIdAsync(Guid id);
        Task<PaginatedList<GetConstructionItemDto>> GetAllAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);
        Task<List<GetConstructionItemDropdownDto>> GetDropdownAsync();
    }
}
