namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class EngineerRequestFilterDto : Contracting.Shared.Dtos.BaseFilterDto
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? StatusId { get; set; }
    }
}
