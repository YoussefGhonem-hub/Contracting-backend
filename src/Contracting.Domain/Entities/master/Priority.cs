using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Priority: BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? code { get; set; }
    }
}
