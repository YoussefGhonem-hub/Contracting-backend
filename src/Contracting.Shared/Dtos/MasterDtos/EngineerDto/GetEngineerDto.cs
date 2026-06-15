using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;

namespace Contracting.Shared.Dtos.MasterDtos.EngineerDto
{
    public class GetEngineerDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? passportNumber { get; set; }
        public string? nationalId { get; set; }
        public string? position { get; set; }
        public int? yearExperience { get; set; }
        public string? phoneNumber { get; set; }
        public string? Email { get; set; }
        public Guid ApplicationUserId { get; set; }
        public List<RoleDropDownDto>? Roles { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public List<GetEngineerProjectDto> Projects { get; set; } = new();
        public List<EngineerDepartmentRoleDto> DepartmentRoles { get; set; } = new();
    }

    public class GetEngineerDropDownDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
    }
}
