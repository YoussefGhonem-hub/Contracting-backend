using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class CreateEngineerSiteReportDto
    {
        // 1. Project
        public Guid? ProjectId { get; set; }

        // 2. Report Date
        public DateTimeOffset? ReportDate { get; set; }

        // 3. Workers by Construction Item
        public ICollection<CreateReportWorkerDto>? Workers { get; set; }

        // 4. Work Performed Today
        public string? WorkPerformedToday { get; set; }

        // 5. Material Details
        public string? MaterialDetails { get; set; }

        // 6. Issues / Delay
        public string? IssuesOrDelays { get; set; }

        // 7. Client Visit Today
        public bool ClientVisitToday { get; set; }

        // 8. Visit Details
        public string? VisitDetails { get; set; }

        // 9. Attachments
        public ICollection<IFormFile>? Attachments { get; set; }
    }

    public class CreateReportWorkerDto
    {
        public Guid ConstructionItemId { get; set; }
        public int Count { get; set; }
    }
}
