namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetEngineerRequestSpecialFieldListItemDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentSpecialFieldId { get; set; }
        public string? fieldName { get; set; }
        public string? fieldType { get; set; }
        public int RowIndex { get; set; }
        public string? value { get; set; }
        public int Order { get; set; }
        public string? ListGroupKey { get; set; }
        public int? ReceivedQuantity { get; set; }
    }
}
