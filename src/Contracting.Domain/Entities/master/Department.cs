using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Department :BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }
        public ICollection<Engineer> Engineers { get; set; }

    }
}
