using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteWorkLogAttachment : BaseAuditableEntity
    {
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? FileSize { get; set; }
        public string? Url { get; set; }

        public Guid EngineerSiteWorkLogId { get; set; }
        public EngineerSiteWorkLog EngineerSiteWorkLog { get; set; }
    }
}
