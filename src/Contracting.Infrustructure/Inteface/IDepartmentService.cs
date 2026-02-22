using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Common;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;

namespace Contracting.Infrustructure.Inteface
{
    public interface IDepartmentService
    {
        Task<GetDepartmentDto> CreateDepartmentAsync(Guid branchId, CreateDepartmentDto department);

        // Update an existing department under a branch
        Task<GetDepartmentDto> UpdateDepartmentAsync(UpdateDepartmentDto department);

        // Get all departments for a branch
        Task<PaginatedList<GetDepartmentDto>> GetDepartmentsByBranchIdAsync(Guid branchId, BaseFilterDto filter, CancellationToken cancellationToken);

        // Remove a department from a branch
        Task<GenericResponse> RemoveDepartmentAsync(Guid branchId, Guid departmentId);
        Task<List<GetDepartmentDto>> DropDownMethodAsync(Guid branchId);

        // Check special fields for a department
        Task<DepartmentSpecialFieldsCheckDto> GetDepartmentSpecialFieldsAsync(Guid departmentId);
    }
}
