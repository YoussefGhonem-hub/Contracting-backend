namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class LaborAttendanceFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public string? Status { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? SupervisorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
    }
}
