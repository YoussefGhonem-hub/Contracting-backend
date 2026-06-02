namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class CreateLaborAttendanceRecordDto
    {
        public string? Name { get; set; }
        public string? JobTitle { get; set; }
        /// <summary>Present, Absent, HalfDay, Overtime</summary>
        public string AttendanceStatus { get; set; } = "Present";
        public decimal DailyRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public string? Notes { get; set; }
    }
}
