namespace Contracting.Shared.Dtos.MasterDtos.DepartmentDtos
{
    public class DepartmentSpecialFieldsCheckDto
    {
        public Guid DepartmentId { get; set; }
        public bool hasSpecialFields { get; set; }
        public List<DepartmentSpecialFieldDto> SpecialFields { get; set; } = new();
    }
}
