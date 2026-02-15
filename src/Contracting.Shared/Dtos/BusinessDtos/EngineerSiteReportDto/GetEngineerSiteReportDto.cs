using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;

namespace Contracting.Shared.BusinessDtos.EngineerSiteReportDto
{
    public class GetEngineerSiteReportDto
    {
        public Guid Id { get; set; }

        // 1. Project
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }

        public Guid EngineerId { get; set; }
        public GetEngineerDto? Engineer { get; set; }

        // 2. Report Date
        public DateTimeOffset ReportDate { get; set; }

        // 3. Workers by Construction Item
        public ICollection<GetReportWorkerDto> Workers { get; set; } = new List<GetReportWorkerDto>();

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
        public ICollection<GetAttachmentDto> Attachments { get; set; } = new List<GetAttachmentDto>();
    }

    public class GetReportWorkerDto
    {
        public Guid Id { get; set; }
        public Guid ConstructionItemId { get; set; }
        public GetConstructionItemDropdownDto? ConstructionItem { get; set; }
        public int Count { get; set; }
    }
}
