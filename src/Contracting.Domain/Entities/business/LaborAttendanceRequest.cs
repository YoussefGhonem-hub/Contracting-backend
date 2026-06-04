using Contracting.Domain.Common;
using Contracting.Domain.Entities.business.enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class LaborAttendanceRequest : BaseAuditableEntity
    {
        public string? RequestNumber { get; set; }
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public string? SiteName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public Guid? SupervisorId { get; set; }
        public Engineer? Supervisor { get; set; }
        public Guid? AssignedToId { get; set; }
        public Engineer? AssignedTo { get; set; }
        public string? Notes { get; set; }
        public LaborAttendanceStatus Status { get; set; } = LaborAttendanceStatus.Draft;

        public ICollection<LaborAttendanceRecord> Records { get; set; } = new List<LaborAttendanceRecord>();
        public ICollection<LaborAttendanceAttachment> Attachments { get; set; } = new List<LaborAttendanceAttachment>();
        public ICollection<LaborAttendanceActivity> Activities { get; set; } = new List<LaborAttendanceActivity>();
    }
}
