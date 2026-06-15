namespace Contracting.Shared.Dtos.MasterDtos.EngineerDto
{
    public class UpdateEngineerDto
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
        public Guid? ChangeDepartmentId { get; set; }
        public List<Guid> Roles { get; set; } = new();
        public List<ProjectAssignDto> Projects { get; set; } = new();
        public List<DepartmentRoleDto>? DepartmentRoles { get; set; }

    }
}
