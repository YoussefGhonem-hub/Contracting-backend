using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class ConstructionItem : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? ItemCode { get; set; }
        public ICollection<ConstructionItemUnit> Units { get; set; } = new List<ConstructionItemUnit>();
    }
}
