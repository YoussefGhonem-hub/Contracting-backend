namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateEngineerRequestSpecialFieldValueDto
    {
        public Guid DepartmentSpecialFieldId { get; set; }
        public string? value { get; set; }
    }
}
