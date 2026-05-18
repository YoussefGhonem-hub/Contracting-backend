using Contracting.Domain.Common;
using Contracting.Domain.Entities.business.enums;

namespace Contracting.Domain.Entities.business
{
    public class LaborAttendanceRecord : BaseAuditableEntity
    {
        public Guid LaborAttendanceRequestId { get; set; }
        public LaborAttendanceRequest? LaborAttendanceRequest { get; set; }
        public string? Name { get; set; }
        public string? JobTitle { get; set; }
        public WorkerAttendanceStatus AttendanceStatus { get; set; }
        public decimal DailyRate { get; set; }
        public decimal OvertimeHours { get; set; }
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }
}
