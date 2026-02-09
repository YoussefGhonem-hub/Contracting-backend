using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class EngineerProject : BaseAuditableEntity
    {
        public Guid EngineerId { get; set; }
        public Engineer? Engineer { get; set; }

        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }
    }
}
