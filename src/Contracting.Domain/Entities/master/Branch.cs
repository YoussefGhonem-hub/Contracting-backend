using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Branch : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? location { get; set; }
        public string? currency { get; set; }
        public ICollection<Department> Departments { get; set; }

    }
}
