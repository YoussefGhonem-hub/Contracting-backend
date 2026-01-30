using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Status : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? Code { get; set; }
        public int orderNumber { get; set; }
        public bool showInDropdown { get; set; }
    }
}
