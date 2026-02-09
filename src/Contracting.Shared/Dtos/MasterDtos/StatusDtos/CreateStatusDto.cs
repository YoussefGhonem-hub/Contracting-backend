namespace Contracting.Shared.Dtos.MasterDtos.StatusDtos
{
    public class CreateStatusDto
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? Code { get; set; }
        public int orderNumber { get; set; }
        public bool showInDropdown { get; set; }
        public string? iconName { get; set; }

    }
}
