using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerSiteReport : BaseAuditableEntity
    {
        public Guid EngineerId { get; set; }
        public Engineer Engineer { get; set; }

        // 1. Project
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }

        // 2. Report Date
        public DateTimeOffset ReportDate { get; set; }

        // 3. Workers by Construction Item (child collection)
        public ICollection<ReportConstructionItemWorker> Workers { get; set; } = new List<ReportConstructionItemWorker>();

        // 4. Work Performed Today (free text)
        public string? WorkPerformedToday { get; set; }

        // 5. Material Details (free text)
        public string? MaterialDetails { get; set; }

        // 6. Issues / Delay
        public string? IssuesOrDelays { get; set; }

        // 7. No Work Today (flag — engineer confirms site was idle, no work performed)
        public bool NoWorkToday { get; set; }

        // 8. Client Visit Today (flag)
        public bool ClientVisitToday { get; set; }

        // 9. Visit Details (if client visit)
        public string? VisitDetails { get; set; }

        // 9. Attachments
        public ICollection<EngineerSiteReportAttachment> Attachments { get; set; } = new List<EngineerSiteReportAttachment>();
    }
}
