using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class ConstructionItemUnit : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public Guid ConstructionItemId { get; set; }
        public ConstructionItem? ConstructionItem { get; set; }
    }
}
