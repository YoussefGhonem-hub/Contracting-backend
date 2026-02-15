using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class ReportConstructionItemWorker : BaseAuditableEntity
    {
        public Guid EngineerSiteReportId { get; set; }
        public EngineerSiteReport? EngineerSiteReport { get; set; }

        public Guid ConstructionItemId { get; set; }
        public ConstructionItem? ConstructionItem { get; set; }

        public int Count { get; set; }
    }
}
