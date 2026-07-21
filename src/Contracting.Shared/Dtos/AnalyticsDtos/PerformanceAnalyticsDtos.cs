namespace Contracting.Shared.Dtos.AnalyticsDtos;

public class PerformanceAnalyticsFilterDto
{
    public Guid?     BranchId     { get; set; }  // admin/super only — scope to one branch
    public Guid?     DepartmentId { get; set; }
    public Guid?     EngineerId   { get; set; }  // admin/super only — scope to one engineer
    public DateTime? FromDate     { get; set; }
    public DateTime? ToDate       { get; set; }
}

// ── Site Engineer ─────────────────────────────────────────────────────────────

public class SiteEngineerPerformanceDto
{
    public Guid    EngineerId       { get; set; }
    public string? NameEn           { get; set; }
    public string? NameAr           { get; set; }
    public string? Position         { get; set; }
    public string? DepartmentNameEn { get; set; }
    public bool    IsTopPerformer   { get; set; }

    // Report Completion
    public double ReportCompletionRate  { get; set; }   // 0–100
    public int    ReportsSubmitted      { get; set; }
    public int    WorkingDaysInPeriod   { get; set; }   // pure calendar working days (no weekends)
    public int    ProjectCount          { get; set; }   // number of projects assigned
    public int    ExpectedReportsCount  { get; set; }   // WorkingDaysInPeriod × ProjectCount

    // Urgent Requests Ratio
    public double UrgentRequestsRatio { get; set; }    // 0–100
    public int    UrgentRequests      { get; set; }
    public int    TotalRequests       { get; set; }

    // Missing Information
    public int    MissingInfoRequests { get; set; }    // requests currently in "Missing Information" status
    public double MissingInfoRatio    { get; set; }    // MissingInfoRequests / TotalRequests × 100

    // Request Quality — of requests that reached a final outcome (Completed or
    // Rejected - a request can never move from Rejected to any other status, so
    // these two are mutually exclusive and exhaustive "closed" outcomes), what
    // percentage were Completed. RequestQualityScore = AcceptedOnFirstTry /
    // TotalFinalizedRequests * 100.
    public double RequestQualityScore     { get; set; } // 0–100
    public int    AcceptedOnFirstTry      { get; set; } // = TotalCompletedRequests (kept as a separate field for API stability)
    public int    TotalCompletedRequests  { get; set; }
    public int    RejectedRequests        { get; set; }
    public int    TotalFinalizedRequests  { get; set; } // TotalCompletedRequests + RejectedRequests - the real denominator
}

// ── Office Engineer ───────────────────────────────────────────────────────────

public class OfficeEngineerPerformanceDto
{
    public Guid    EngineerId       { get; set; }
    public string? NameEn           { get; set; }
    public string? NameAr           { get; set; }
    public string? Position         { get; set; }
    public string? DepartmentNameEn { get; set; }

    // Response Time
    public double AvgResponseTimeHours { get; set; }   // hours, target < 8

    // Total requests assigned to this engineer in the period (regardless of status).
    // Used to distinguish "no requests assigned" from "assigned but not yet completed" -
    // the metrics below default to their best possible value when there's no data, so
    // this flags engineers who shouldn't be averaged in as if they were flawless.
    public int TotalRequests { get; set; }

    // On-Time Delivery
    public double OnTimeDeliveryRate { get; set; }     // 0–100
    public int    OnTimeDeliveries   { get; set; }
    public int    TotalDeliveries    { get; set; }

    // Delivery Date Violations
    public int DeliveryDateViolations { get; set; }    // count, target = 0

    // Weekly Activity Log
    public List<WeeklyActivityDto> WeeklyActivity { get; set; } = new();
}

public class WeeklyActivityDto
{
    public string? ProjectRef  { get; set; }  // project nameEn
    public string? Task        { get; set; }  // request title
    public string? Complexity  { get; set; }  // priority nameEn
    public string? Status      { get; set; }  // status nameEn
    public string? StatusCode  { get; set; }  // NEW | IN_PROGRESS | COMPLETED | REJECTED
}

// ── Department Performance Index ──────────────────────────────────────────────

public class DeptPerformanceIndexDto
{
    public string? Grade            { get; set; }  // A+, A, A-, B+, B, B-, C
    public double  OverallScore     { get; set; }  // 0–100
    public string? BenchmarkMessage { get; set; }
}

// ── Full Response ─────────────────────────────────────────────────────────────

public class PerformanceAnalyticsResponseDto
{
    public string? DepartmentNameEn { get; set; }
    public DateTime FromDate        { get; set; }
    public DateTime ToDate          { get; set; }

    public List<SiteEngineerPerformanceDto>   SiteEngineers   { get; set; } = new();
    public List<OfficeEngineerPerformanceDto> OfficeEngineers { get; set; } = new();
    public DeptPerformanceIndexDto?           DeptIndex       { get; set; }
}

// ── Full Report ───────────────────────────────────────────────────────────────

public class FullReportSummaryDto
{
    public int    TotalSiteEngineers    { get; set; }
    public int    TotalOfficeEngineers  { get; set; }
    public int    TotalRequests         { get; set; }
    public int    TotalCompleted        { get; set; }
    public int    TotalHighPriority     { get; set; }
    public int    TotalReportsSubmitted { get; set; }
    public double OverallCompletionRate { get; set; }
    public double OverallQualityScore   { get; set; }
    public double OverallOnTimeRate     { get; set; }
    public string? TopPerformerNameEn  { get; set; }
    public string? TopPerformerNameAr  { get; set; }
}

public class ScoreBreakdownDto
{
    public double ReportCompletionScore  { get; set; }
    public double RequestQualityScore    { get; set; }
    public double HighPriorityScore      { get; set; }  // inverted: lower ratio = higher score
    public double OnTimeDeliveryScore    { get; set; }
    public double ResponseTimeScore      { get; set; }
    public double ViolationsScore        { get; set; }
}

public class SiteEngineerFullDto : SiteEngineerPerformanceDto
{
    public int    LowPriorityRequests    { get; set; }
    public int    MediumPriorityRequests { get; set; }
    public int    HighPriorityRequests   { get; set; }
    // RejectedRequests/TotalFinalizedRequests are inherited from SiteEngineerPerformanceDto.
    public double CompositeScore         { get; set; }
}

public class OfficeEngineerFullDto : OfficeEngineerPerformanceDto
{
    public int    PendingRequests  { get; set; }
    public int    InProgressCount  { get; set; }
    public double CompositeScore   { get; set; }
}

public class ReportInsightDto
{
    public string Type    { get; set; } = "info";  // info | warning | success
    public string Message { get; set; } = "";
}

public class FullPerformanceReportDto
{
    public string?   DepartmentNameEn { get; set; }
    public DateTime  GeneratedAt      { get; set; }
    public DateTime  FromDate         { get; set; }
    public DateTime  ToDate           { get; set; }

    public DeptPerformanceIndexDto? DeptIndex      { get; set; }
    public ScoreBreakdownDto        ScoreBreakdown { get; set; } = new();
    public FullReportSummaryDto     Summary        { get; set; } = new();

    public List<SiteEngineerFullDto>   SiteEngineers   { get; set; } = new();
    public List<OfficeEngineerFullDto> OfficeEngineers { get; set; } = new();
    public List<ReportInsightDto>      Insights        { get; set; } = new();
}
