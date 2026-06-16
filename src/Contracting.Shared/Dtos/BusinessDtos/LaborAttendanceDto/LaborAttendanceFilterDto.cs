namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class LaborAttendanceFilterDto
    {
        public int PageIndex { get; set; } = 1;
        public int PageSize { get; set; } = 10;
        public Guid? StatusId { get; set; }
        /// <summary>Status name or code to filter by (e.g. "Rejected"). Resolved to a StatusId server-side.</summary>
        public string? Status { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? SupervisorId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
    }
}
