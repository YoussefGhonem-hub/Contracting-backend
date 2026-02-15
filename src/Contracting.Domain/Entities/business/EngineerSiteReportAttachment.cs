using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteReportAttachment : BaseAuditableEntity
    {
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? FileSize { get; set; }
        public string? Url { get; set; }

        public Guid EngineerSiteReportId { get; set; }
        public EngineerSiteReport? EngineerSiteReport { get; set; }
    }
}
