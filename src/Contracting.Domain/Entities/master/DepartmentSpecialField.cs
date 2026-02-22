using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class DepartmentSpecialField : BaseAuditableEntity
    {
        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Guid SpecialFieldId { get; set; }
        public SpecialField? SpecialField { get; set; }

        public string? value { get; set; }
    }
}
