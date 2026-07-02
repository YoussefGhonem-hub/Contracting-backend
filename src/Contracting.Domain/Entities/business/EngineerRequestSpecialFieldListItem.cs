using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequestSpecialFieldListItem : BaseAuditableEntity
    {
        public Guid EngineerRequestId { get; set; }
        public EngineerRequest? EngineerRequest { get; set; }

        public Guid DepartmentSpecialFieldId { get; set; }
        public DepartmentSpecialField? DepartmentSpecialField { get; set; }

        /// <summary>Groups cells belonging to the same repeated row across different list-flagged fields.</summary>
        public int RowIndex { get; set; }

        public string? value { get; set; }
    }
}
