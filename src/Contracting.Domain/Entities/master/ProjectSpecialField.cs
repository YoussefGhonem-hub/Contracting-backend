using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class ProjectSpecialField : BaseAuditableEntity
    {
        public Guid ProjectId { get; set; }
        public Project? Project { get; set; }

        public Guid SpecialFieldId { get; set; }
        public SpecialField? SpecialField { get; set; }

        public string? value { get; set; }
    }
}