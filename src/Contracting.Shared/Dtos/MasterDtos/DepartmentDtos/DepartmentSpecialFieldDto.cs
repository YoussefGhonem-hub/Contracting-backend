namespace Contracting.Shared.Dtos.MasterDtos.DepartmentDtos
{
    public class DepartmentSpecialFieldDto
    {
        public Guid Id { get; set; }
        public Guid SpecialFieldId { get; set; }
        public string? name { get; set; }
        public string? fieldType { get; set; }
        public string? value { get; set; }
        public int Order { get; set; }
        public int ColSpan { get; set; } = 1;
        public string? ListGroupKey { get; set; }
    }
}
