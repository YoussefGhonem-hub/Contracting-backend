using Contracting.Domain.Common;
using Contracting.Domain.Entities.business.enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class LaborAttendanceActivity : BaseAuditableEntity
    {
        public Guid LaborAttendanceRequestId { get; set; }
        public LaborAttendanceRequest? LaborAttendanceRequest { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public LaborAttendanceStatus? FromStatus { get; set; }
        public LaborAttendanceStatus ToStatus { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
    }
}
