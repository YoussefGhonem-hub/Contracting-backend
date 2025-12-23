using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Project : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? location { get; set; }
        public string? Code { get; set; }
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }
    }
}
