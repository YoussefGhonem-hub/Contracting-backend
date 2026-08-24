using Contracting.Shared.Common;
using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IRequestTypeDefaultDepartmentService
    {
        Task<GetRequestTypeDefaultDepartmentDto?> CreateAsync(CreateRequestTypeDefaultDepartmentDto dto);

        Task<GetRequestTypeDefaultDepartmentDto?> UpdateAsync(UpdateRequestTypeDefaultDepartmentDto dto);

        Task<GenericResponse> DeleteAsync(Guid id);

        Task<List<GetRequestTypeDefaultDepartmentDto>> GetAllByBranchAsync(Guid branchId);

        Task<GetDefaultDepartmentDto> GetDefaultAsync(Guid branchId, string requestType);
    }
}
