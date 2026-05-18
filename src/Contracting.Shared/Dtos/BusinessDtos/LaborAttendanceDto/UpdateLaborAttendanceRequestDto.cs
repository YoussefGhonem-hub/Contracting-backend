using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class UpdateLaborAttendanceRequestDto
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public string? SiteName { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public string? Notes { get; set; }
        public List<CreateLaborAttendanceRecordDto>? Records { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}
