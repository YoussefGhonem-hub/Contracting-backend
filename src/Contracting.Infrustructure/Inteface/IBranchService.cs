using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.MasterDtos.BranchDto;

namespace Contracting.Infrustructure.Inteface
{
    public interface IBranchService
    {
        Task<GetBranchDto> AddAsync(CreateBranchDto branch);
        // Read
        Task<GetBranchDto> GetByIdAsync(Guid id);
        Task<PaginatedList<GetBranchDto>> GetAllAsync(BaseFilterDto filter, CancellationToken cancellationToken = default);
        // Update
        Task<GetBranchDto> UpdateAsync(UpdateBranchDto branch);
        // Delete
        Task<GenericResponse> DeleteAsync(Guid id);
        Task<List<BranchDropDownDto>> DropDownMethodAsync();

    }
}
