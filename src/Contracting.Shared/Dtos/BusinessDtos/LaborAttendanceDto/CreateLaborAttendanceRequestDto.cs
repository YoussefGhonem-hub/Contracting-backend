using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class CreateLaborAttendanceRequestDto
    {
        public Guid? ProjectId { get; set; }
        public string? SiteName { get; set; }
        public DateTime AttendanceDate { get; set; } = DateTime.UtcNow;
        public string? Notes { get; set; }
        public List<CreateLaborAttendanceRecordDto> Records { get; set; } = new();
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}
