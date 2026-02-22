namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class EngineerRequestSpecialFieldValueDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentSpecialFieldId { get; set; }
        public string? fieldName { get; set; }
        public string? fieldType { get; set; }
        public string? value { get; set; }
    }
}
