namespace Contracting.Shared.Dtos.MasterDtos.DepartmentDtos
{
    public class CreateDepartmentDto
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public bool hasSpecialFields { get; set; }
        public List<CreateDepartmentSpecialFieldDto> SpecialFields { get; set; } = new();
    }
}
