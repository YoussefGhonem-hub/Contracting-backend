namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class GetLaborAttendanceRecordDto
    {
        public Guid Id { get; set; }
        public string? Name { get; set; }
        public string? JobTitle { get; set; }
        public string? AttendanceStatus { get; set; }
        public decimal DailyRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }
}
