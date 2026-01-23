using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteMaterial : BaseAuditableEntity
    {
        public Guid EngineerSiteReportId { get; set; }
        public EngineerSiteReport EngineerSiteReport { get; set; }
        public string? name { get; set; }
        public decimal? quantity { get; set; }
        public string? usage { get; set; }
        public bool? needMore { get; set; }
        public string? unit { get; set; }
        public decimal? unitCost { get; set; }
        public decimal? totalCost { get; set; }
        public string? notes { get; set; }
    }
}
