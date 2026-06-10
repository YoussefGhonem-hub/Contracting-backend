using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetEngineerRequestSpecialFieldItemDto
    {
        public Guid Id { get; set; }
        public Guid DepartmentSpecialFieldId { get; set; }
        public Guid ConstructionItemId { get; set; }
        public int Quantity { get; set; }
        public GetConstructionItemDto? ConstructionItem { get; set; }
    }
}
