namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateEngineerRequestSpecialFieldValueDto
    {
        public Guid ProjectSpecialFieldId { get; set; }
        public string? value { get; set; }
    }
}
