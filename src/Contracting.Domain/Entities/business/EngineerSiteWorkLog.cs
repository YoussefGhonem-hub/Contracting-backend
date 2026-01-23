using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteWorkLog : BaseAuditableEntity
    {
        public Guid EngineerSiteReportId { get; set; }
        public EngineerSiteReport EngineerSiteReport { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        public decimal? quantity { get; set; }
        public decimal? totalHours { get; set; }
        public decimal? totalHoursToDate { get; set; }

        public ICollection<EngineerSiteWorkLogAttachment> Attachments { get; set; }
    }
}
