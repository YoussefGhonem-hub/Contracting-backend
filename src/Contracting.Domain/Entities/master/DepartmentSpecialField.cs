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

        /// <summary>Display order within the department (0-based).</summary>
        public int Order { get; set; }

        /// <summary>How many columns this field spans in a 4-column grid (1–4). Default 1.</summary>
        public int ColSpan { get; set; } = 1;

        /// <summary>When set, this field belongs to a repeatable list group; fields sharing the same key form one list. Null/empty means a single-value field.</summary>
        public string? ListGroupKey { get; set; }
    }
}
