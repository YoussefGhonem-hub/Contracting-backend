using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequestNotes : BaseAuditableEntity
    {
        public string? note { get; set; }
        public Guid? EngineerRequestId { get; set; }
        public EngineerRequest? EngineerRequest { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }

    }
}
