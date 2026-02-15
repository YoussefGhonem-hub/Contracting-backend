using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class ConstructionItem : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
    }
}
