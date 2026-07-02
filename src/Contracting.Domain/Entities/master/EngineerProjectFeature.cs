using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class EngineerProjectFeature : BaseEntity
    {
        public Guid EngineerProjectId { get; set; }
        public EngineerProject? EngineerProject { get; set; }

        public string Feature { get; set; } = string.Empty;
    }
}
