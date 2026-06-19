namespace Contracting.Shared.Dtos.AnalyticsDtos;

public class PerformanceAnalyticsFilterDto
{
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
    public double ReportCompletionRate { get; set; }   // 0–100
    public int    ReportsSubmitted     { get; set; }
    public int    WorkingDaysInPeriod  { get; set; }

    // Urgent Requests Ratio
    public double UrgentRequestsRatio { get; set; }    // 0–100
    public int    UrgentRequests      { get; set; }
    public int    TotalRequests       { get; set; }

    // Request Quality
    public double RequestQualityScore    { get; set; } // 0–100
    public int    AcceptedOnFirstTry     { get; set; }
    public int    TotalCompletedRequests { get; set; }
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
