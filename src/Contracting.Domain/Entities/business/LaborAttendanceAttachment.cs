using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class LaborAttendanceAttachment : BaseAuditableEntity
    {
        public Guid LaborAttendanceRequestId { get; set; }
        public LaborAttendanceRequest? LaborAttendanceRequest { get; set; }
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? FileSize { get; set; }
        public string? Url { get; set; }
    }
}
