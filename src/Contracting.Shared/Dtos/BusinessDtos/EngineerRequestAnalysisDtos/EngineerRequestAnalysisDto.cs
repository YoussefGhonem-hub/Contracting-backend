namespace Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos
{
    public class PriorityCountDto
    {
        public Guid? PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class SiteEngineerAnalysisDto
    {
        public int TotalRequests { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedOverDeadline { get; set; }
        public List<PriorityCountDto> RequestsByPriority { get; set; } = new();
    }

    public class OfficeEngineerAnalysisDto
    {
        public int TotalRequests { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedOverDeadline { get; set; }
        public int ActiveWithinDeadlineCount { get; set; }
        public decimal ActiveWithinDeadlinePercentage { get; set; }
    }

    public class TeamLeadAnalysisDto
    {
        public int TotalRequests { get; set; }
        public int CompletedOnTime { get; set; }
        public int CompletedOverDeadline { get; set; }
        public int OnHoldCount { get; set; }
        public int NotFinishedCount { get; set; }
    }
}
