namespace Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos
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

    public class SlaBucketByPriorityDepartmentDto
    {
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public Guid? PriorityId { get; set; }
        public string PriorityName { get; set; } = string.Empty;
        public int CompletedCount { get; set; }
        public int EarlyCount { get; set; }
        public int OnTimeCount { get; set; }
        public int LateCount { get; set; }
        public decimal EarlyPercentage { get; set; }
        public decimal OnTimePercentage { get; set; }
        public decimal LatePercentage { get; set; }
    }

    public class SlaBucketsReportDto
    {
        public int TotalCompletedWithDeadline { get; set; }
        public List<SlaBucketByPriorityDepartmentDto> Buckets { get; set; } = new();
    }

    public class AgingBucketDto
    {
        public string RangeLabel { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class AgingReportDto
    {
        public int OpenRequests { get; set; }
        public List<AgingBucketDto> Buckets { get; set; } = new();
    }

    public class LeadCycleTimeDto
    {
        public int CompletedRequests { get; set; }
        public decimal AverageLeadTimeDays { get; set; }
        public decimal AverageCycleTimeDays { get; set; }
    }

    public class OverdueRiskDto
    {
        public int OpenRequestsWithDeadline { get; set; }
        public int AtRiskCount { get; set; }
        public decimal AtRiskPercentage { get; set; }
    }

    public class AssigneePerformanceDto
    {
        public Guid? EngineerId { get; set; }
        public string EngineerName { get; set; } = string.Empty;
        public Guid? DepartmentId { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public int TotalAssigned { get; set; }
        public int CompletedAssigned { get; set; }
        public decimal CompletionRate { get; set; }
        public decimal AverageCompletionDays { get; set; }
    }

    public class AssigneePerformanceReportDto
    {
        public int TotalAssignedRequests { get; set; }
        public List<AssigneePerformanceDto> Assignees { get; set; } = new();
    }

    public class ReworkRateDto
    {
        public int RequestsWithCompletion { get; set; }
        public int ReworkedRequests { get; set; }
        public decimal ReworkPercentage { get; set; }
    }

    public class EngineerStatusPercentageDto
    {
        public Guid? EngineerId { get; set; }
        public string EngineerName { get; set; } = string.Empty;
        public int TotalRequests { get; set; }

        public int FinishedInTimeCount { get; set; }
        public decimal FinishedInTimePercentage { get; set; }

        public int OnHoldCount { get; set; }
        public decimal OnHoldPercentage { get; set; }

        public int DelayedCount { get; set; }
        public decimal DelayedPercentage { get; set; }

        public int CompletedCount { get; set; }
        public decimal CompletedPercentage { get; set; }
    }

    public class EngineerStatusPercentageReportDto
    {
        public int TotalRequests { get; set; }
        public List<EngineerStatusPercentageDto> Engineers { get; set; } = new();
    }
}
