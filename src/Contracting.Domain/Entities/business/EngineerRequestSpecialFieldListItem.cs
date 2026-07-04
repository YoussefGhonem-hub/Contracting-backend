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

        /// <summary>
        /// Goods-receipt tracking for list-group fields that represent a quantity (e.g. "Qty
        /// Required" in a Procurement materials list). Only meaningful for the numeric
        /// "quantity" cell within a row; other cells in the same row (S/N, description, unit,
        /// item code, etc.) leave this null.
        /// </summary>
        public int? ReceivedQuantity { get; set; }
    }
}
