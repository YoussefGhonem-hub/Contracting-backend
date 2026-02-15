namespace Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos
{
    public class CreateConstructionItemDto
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }

    public class UpdateConstructionItemDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }

    public class GetConstructionItemDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }

    public class GetConstructionItemDropdownDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }
}
