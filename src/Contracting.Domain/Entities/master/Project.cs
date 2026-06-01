using Contracting.Domain.Common;
using Contracting.Shared.Common.Enums;

namespace Contracting.Domain.Entities.master
{
    public class Project : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? location { get; set; }
        public string? Code { get; set; }
        public string? imageUrl { get; set; }
        public string? imageKey { get; set; }
        public decimal? Area { get; set; }
        public decimal? ContractValue { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? ExpectedEndDate { get; set; }
        public ProjectStatus? ProjectStatus { get; set; }
        public Guid? BranchId { get; set; }
        public Branch? Branch { get; set; }
        public ICollection<EngineerProject> EngineerProjects { get; set; } = new List<EngineerProject>();
    }
}
