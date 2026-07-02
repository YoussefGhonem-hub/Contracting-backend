namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateEngineerRequestSpecialFieldListItemDto
    {
        public Guid DepartmentSpecialFieldId { get; set; }
        public int RowIndex { get; set; }
        public string? value { get; set; }
    }
}
