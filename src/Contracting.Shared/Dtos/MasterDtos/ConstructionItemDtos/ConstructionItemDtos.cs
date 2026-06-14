namespace Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos
{
    public class ConstructionItemUnitDto
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }

    public class CreateConstructionItemDto
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? ItemCode { get; set; }
        public List<ConstructionItemUnitDto> Units { get; set; } = new();
    }

    public class UpdateConstructionItemDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? ItemCode { get; set; }
        public List<ConstructionItemUnitDto> Units { get; set; } = new();
    }

    public class GetConstructionItemDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? ItemCode { get; set; }
        public List<ConstructionItemUnitDto> Units { get; set; } = new();
    }

    public class GetConstructionItemDropdownDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? ItemCode { get; set; }
        public List<ConstructionItemUnitDto> Units { get; set; } = new();
    }
}
