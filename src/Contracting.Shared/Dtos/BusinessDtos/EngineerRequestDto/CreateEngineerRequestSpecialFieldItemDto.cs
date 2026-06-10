namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateEngineerRequestSpecialFieldItemDto
    {
        public Guid DepartmentSpecialFieldId { get; set; }
        public Guid ConstructionItemId { get; set; }
        public int Quantity { get; set; }
    }
}
