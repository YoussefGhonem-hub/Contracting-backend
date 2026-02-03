namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetEngineerRequestCountByStatusDto
    {
        public Guid StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string StatusNameAr { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
