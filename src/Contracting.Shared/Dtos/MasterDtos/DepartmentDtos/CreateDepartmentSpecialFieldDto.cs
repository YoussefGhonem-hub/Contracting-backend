namespace Contracting.Shared.Dtos.MasterDtos.DepartmentDtos
{
    public class CreateDepartmentSpecialFieldDto
    {
        public string? name { get; set; }
        public string? fieldType { get; set; }
        public string? value { get; set; }
        public int Order { get; set; }
        public int ColSpan { get; set; } = 1;
    }
}
