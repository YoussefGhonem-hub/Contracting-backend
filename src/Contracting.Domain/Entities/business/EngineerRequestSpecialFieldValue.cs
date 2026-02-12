using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequestSpecialFieldValue : BaseAuditableEntity
    {
        public Guid EngineerRequestId { get; set; }
        public EngineerRequest? EngineerRequest { get; set; }

        public Guid ProjectSpecialFieldId { get; set; }
        public ProjectSpecialField? ProjectSpecialField { get; set; }

        public string? value { get; set; }
    }
}
