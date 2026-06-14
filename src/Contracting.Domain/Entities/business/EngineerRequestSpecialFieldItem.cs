using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequestSpecialFieldItem : BaseAuditableEntity
    {
        public Guid EngineerRequestId { get; set; }
        public EngineerRequest? EngineerRequest { get; set; }

        public Guid DepartmentSpecialFieldId { get; set; }
        public DepartmentSpecialField? DepartmentSpecialField { get; set; }

        public Guid ConstructionItemId { get; set; }
        public ConstructionItem? ConstructionItem { get; set; }

        public int Quantity { get; set; }
        public int? ReceivedQuantity { get; set; }
    }
}
