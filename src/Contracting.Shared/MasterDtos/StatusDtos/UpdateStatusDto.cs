namespace Contracting.Shared.MasterDtos.StatusDtos
{
    public class UpdateStatusDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? Code { get; set; }
        public int orderNumber { get; set; }

    }
}
