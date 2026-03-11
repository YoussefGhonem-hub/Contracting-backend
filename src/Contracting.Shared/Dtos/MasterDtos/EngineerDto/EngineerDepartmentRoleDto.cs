using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;

namespace Contracting.Shared.Dtos.MasterDtos.EngineerDto
{
    public class EngineerDepartmentRoleDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public Guid RoleId { get; set; }
        public RoleDropDownDto? Role { get; set; }
    }
}
