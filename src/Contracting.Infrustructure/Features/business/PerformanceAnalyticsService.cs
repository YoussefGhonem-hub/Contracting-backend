using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.AnalyticsDtos;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business;

public class PerformanceAnalyticsService : IPerformanceAnalyticsService
{
    private readonly ApplicationDbContext _db;

    public PerformanceAnalyticsService(ApplicationDbContext db)
    {
        _db = db;
    }

    // ── Public entry point ────────────────────────────────────────────────────

    public async Task<ErrorOr<PerformanceAnalyticsResponseDto>> GetSiteAnalyticsAsync(
        PerformanceAnalyticsFilterDto filter, CancellationToken ct = default)
    {
        var from = filter.FromDate ?? DateTime.UtcNow.AddMonths(-1).Date;
        var to   = (filter.ToDate  ?? DateTime.UtcNow).Date.AddDays(1); // exclusive

        // Resolve which engineer IDs the caller is allowed to see
        var (allowedIds, deptName) = await ResolveAllowedEngineersAsync(filter, ct);
        if (allowedIds is null)
            return Error.Forbidden("Performance.Forbidden", "You do not have access to this data.");

        // Split by role
        var siteIds   = await FilterByRoleAsync(allowedIds, RoleNames.Siteengineer,   ct);
        var officeIds = await FilterByRoleAsync(allowedIds, RoleNames.Officeengineer, ct);

        var siteMetrics   = await BuildSiteMetricsAsync(siteIds,   from, to, ct);
        var officeMetrics = await BuildOfficeMetricsAsync(officeIds, from, to, ct);

        // Mark top performer (highest composite score)
        MarkTopPerformer(siteMetrics);

        var deptIndex = BuildDeptIndex(siteMetrics, officeMetrics);

        return new PerformanceAnalyticsResponseDto
        {
            DepartmentNameEn = deptName,
            FromDate         = from,
            ToDate           = to.AddDays(-1),
            SiteEngineers    = siteMetrics,
            OfficeEngineers  = officeMetrics,
            DeptIndex        = deptIndex
        };
    }

    // ── Authorization: resolve which engineers the caller can see ─────────────

    private async Task<(List<Guid>? ids, string? deptName)> ResolveAllowedEngineersAsync(
        PerformanceAnalyticsFilterDto filter, CancellationToken ct)
    {
        var roles = CurrentUser.Roles;

        bool isSuperOrAdmin = roles.Contains(RoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase)
                           || roles.Contains(RoleNames.Admin,      StringComparer.OrdinalIgnoreCase);

        bool isTeamLead  = roles.Contains(RoleNames.Teamleadengineer, StringComparer.OrdinalIgnoreCase);
        bool isSelf      = roles.Contains(RoleNames.Siteengineer,     StringComparer.OrdinalIgnoreCase)
                        || roles.Contains(RoleNames.Officeengineer,    StringComparer.OrdinalIgnoreCase);

        // ── Super / Admin ──────────────────────────────────────────────────
        if (isSuperOrAdmin)
        {
            var query = _db.Engineers.Where(e => !e.IsDeleted);

            string? deptName = null;
            if (filter.DepartmentId.HasValue)
            {
                query    = query.Where(e => e.DepartmentId == filter.DepartmentId.Value);
                deptName = await _db.Departmentes
                    .Where(d => d.Id == filter.DepartmentId.Value)
                    .Select(d => d.nameEn)
                    .FirstOrDefaultAsync(ct);
            }

            if (filter.EngineerId.HasValue)
                query = query.Where(e => e.Id == filter.EngineerId.Value);

            var ids = await query.Select(e => e.Id).ToListAsync(ct);
            return (ids, deptName);
        }

        // ── Team Lead ──────────────────────────────────────────────────────
        if (isTeamLead)
        {
            if (!Guid.TryParse(CurrentUser.UserId, out var userId))
                return (null, null);

            // Find caller's primary department
            var leadEngineer = await _db.Engineers
                .FirstOrDefaultAsync(e => e.ApplicationUserId == userId && !e.IsDeleted, ct);
            if (leadEngineer is null) return (null, null);

            var deptId = leadEngineer.DepartmentId;
            if (deptId is null) return (new List<Guid>(), null);

            var deptName = await _db.Departmentes
                .Where(d => d.Id == deptId)
                .Select(d => d.nameEn)
                .FirstOrDefaultAsync(ct);

            var query = _db.Engineers
                .Where(e => e.DepartmentId == deptId && !e.IsDeleted);

            // Team lead can further scope to a specific engineer in their dept
            if (filter.EngineerId.HasValue)
                query = query.Where(e => e.Id == filter.EngineerId.Value);

            var ids = await query.Select(e => e.Id).ToListAsync(ct);
            return (ids, deptName);
        }

        // ── Site/Office Engineer — see only self ───────────────────────────
        if (isSelf)
        {
            if (!Guid.TryParse(CurrentUser.UserId, out var userId))
                return (null, null);

            var engineer = await _db.Engineers
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.ApplicationUserId == userId && !e.IsDeleted, ct);
            if (engineer is null) return (null, null);

            return (new List<Guid> { engineer.Id }, engineer.Department?.nameEn);
        }

        return (null, null);
    }

    // ── Filter engineer IDs by ASP.NET Core role ──────────────────────────────

    private async Task<List<Guid>> FilterByRoleAsync(
        List<Guid> engineerIds, string roleName, CancellationToken ct)
    {
        if (engineerIds.Count == 0) return new List<Guid>();

        var roleId = await _db.Roles
            .Where(r => r.Name == roleName)
            .Select(r => r.Id)
            .FirstOrDefaultAsync(ct);

        if (roleId == Guid.Empty) return new List<Guid>();

        var appUserIds = await _db.Engineers
            .Where(e => engineerIds.Contains(e.Id) && !e.IsDeleted)
            .Select(e => e.ApplicationUserId)
            .ToListAsync(ct);

        var usersInRole = (await _db.UserRoles
            .Where(ur => appUserIds.Contains(ur.UserId) && ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .ToListAsync(ct)).ToHashSet();

        return await _db.Engineers
            .Where(e => engineerIds.Contains(e.Id) && usersInRole.Contains(e.ApplicationUserId))
            .Select(e => e.Id)
            .ToListAsync(ct);
    }

    // ── Site Engineer Metrics ─────────────────────────────────────────────────

    private async Task<List<SiteEngineerPerformanceDto>> BuildSiteMetricsAsync(
        List<Guid> engineerIds, DateTime from, DateTime to, CancellationToken ct)
    {
        if (engineerIds.Count == 0) return new List<SiteEngineerPerformanceDto>();

        var workingDays = CountWorkingDays(from, to.AddDays(-1));

        // Bulk-load engineers
        var engineers = await _db.Engineers
            .Include(e => e.Department)
            .Where(e => engineerIds.Contains(e.Id) && !e.IsDeleted)
            .ToListAsync(ct);

        // Bulk-load site reports in period
        var reports = await _db.EngineerSiteReports
            .Where(r => engineerIds.Contains(r.EngineerId)
                     && r.ReportDate >= from && r.ReportDate < to
                     && !r.IsDeleted)
            .Select(r => new { r.EngineerId, r.ReportDate })
            .ToListAsync(ct);

        // Bulk-load requests created by these engineers in period
        var requests = await _db.EngineerRequests
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.EngineerRequestActivites)
            .Where(r => r.EngineerId != null
                     && engineerIds.Contains(r.EngineerId!.Value)
                     && r.CreatedDate >= from && r.CreatedDate < to
                     && !r.IsDeleted)
            .ToListAsync(ct);

        var result = new List<SiteEngineerPerformanceDto>();

        foreach (var eng in engineers)
        {
            var myReports  = reports.Where(r => r.EngineerId == eng.Id).ToList();
            var myRequests = requests.Where(r => r.EngineerId == eng.Id).ToList();

            // Report Completion
            int submitted = myReports.Count;
            double completionRate = workingDays > 0
                ? Math.Round((double)submitted / workingDays * 100, 1)
                : 0;

            // Urgent Requests Ratio
            int total  = myRequests.Count;
            int urgent = myRequests.Count(r =>
                r.Priority != null &&
                (r.Priority.nameEn?.Contains("urgent", StringComparison.OrdinalIgnoreCase) == true
              || r.Priority.code?.Contains("urgent", StringComparison.OrdinalIgnoreCase)   == true));
            double urgentRatio = total > 0
                ? Math.Round((double)urgent / total * 100, 1)
                : 0;

            // Request Quality Score — completed without being rejected first
            var completed = myRequests
                .Where(r => r.Status?.Code == MasterStatusCodes.Completed)
                .ToList();

            int acceptedFirstTry = completed.Count(r =>
                !r.EngineerRequestActivites.Any(a =>
                    a.Status?.Code == MasterStatusCodes.Rejected && !a.IsDeleted));

            double qualityScore = completed.Count > 0
                ? Math.Round((double)acceptedFirstTry / completed.Count * 100, 1)
                : 100;

            result.Add(new SiteEngineerPerformanceDto
            {
                EngineerId            = eng.Id,
                NameEn                = eng.nameEn,
                NameAr                = eng.nameAr,
                Position              = eng.position,
                DepartmentNameEn      = eng.Department?.nameEn,
                ReportCompletionRate  = completionRate,
                ReportsSubmitted      = submitted,
                WorkingDaysInPeriod   = workingDays,
                UrgentRequestsRatio   = urgentRatio,
                UrgentRequests        = urgent,
                TotalRequests         = total,
                RequestQualityScore   = qualityScore,
                AcceptedOnFirstTry    = acceptedFirstTry,
                TotalCompletedRequests = completed.Count
            });
        }

        return result.OrderByDescending(e => e.ReportCompletionRate).ToList();
    }

    // ── Office Engineer Metrics ───────────────────────────────────────────────

    private async Task<List<OfficeEngineerPerformanceDto>> BuildOfficeMetricsAsync(
        List<Guid> engineerIds, DateTime from, DateTime to, CancellationToken ct)
    {
        if (engineerIds.Count == 0) return new List<OfficeEngineerPerformanceDto>();

        var engineers = await _db.Engineers
            .Include(e => e.Department)
            .Where(e => engineerIds.Contains(e.Id) && !e.IsDeleted)
            .ToListAsync(ct);

        // Requests assigned TO these office engineers in period
        var requests = await _db.EngineerRequests
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.Project)
            .Include(r => r.EngineerRequestActivites.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Status)
            .Where(r => r.assignToId != null
                     && engineerIds.Contains(r.assignToId!.Value)
                     && r.CreatedDate >= from && r.CreatedDate < to
                     && !r.IsDeleted)
            .ToListAsync(ct);

        var result = new List<OfficeEngineerPerformanceDto>();

        foreach (var eng in engineers)
        {
            var myRequests = requests.Where(r => r.assignToId == eng.Id).ToList();

            // Avg Response Time — from request creation to first activity by this engineer
            var responseTimes = new List<double>();
            foreach (var req in myRequests)
            {
                var firstResponse = req.EngineerRequestActivites
                    .Where(a => a.EngineerId == eng.Id
                             && (a.ActionType == nameof(EngineerRequestActionType.StatusChanged)
                              || a.ActionType == nameof(EngineerRequestActionType.Assigned)))
                    .OrderBy(a => a.CreatedDate)
                    .FirstOrDefault();

                if (firstResponse != null)
                {
                    var hours = (firstResponse.CreatedDate - req.CreatedDate).TotalHours;
                    if (hours >= 0) responseTimes.Add(hours);
                }
            }
            double avgResponse = responseTimes.Count > 0
                ? Math.Round(responseTimes.Average(), 1)
                : 0;

            // On-Time Delivery
            var completed = myRequests
                .Where(r => r.Status?.Code == MasterStatusCodes.Completed)
                .ToList();

            int onTime = completed.Count(r =>
                r.endDate.HasValue &&
                (r.ModifiedDate ?? r.CreatedDate) <= r.endDate.Value.ToUniversalTime());

            double onTimeRate = completed.Count > 0
                ? Math.Round((double)onTime / completed.Count * 100, 1)
                : 100;

            // Delivery Date Violations — confirmed date changed after confirmation
            int violations = myRequests.Count(r =>
                r.IsDeliveryDateConfirmed &&
                r.EngineerRequestActivites.Any(a =>
                    a.ActionType == nameof(EngineerRequestActionType.Reassigned) ||
                    a.ActionType == nameof(EngineerRequestActionType.StatusChangedAuto)));

            // Weekly Activity Log — last 7 days of the period
            var weekStart = to.AddDays(-7);
            var weekActivity = myRequests
                .Where(r => r.CreatedDate >= weekStart)
                .OrderByDescending(r => r.CreatedDate)
                .Take(20)
                .Select(r => new WeeklyActivityDto
                {
                    ProjectRef = r.Project?.nameEn,
                    Task       = r.RequestTitle,
                    Complexity = r.Priority?.nameEn,
                    Status     = r.Status?.nameEn,
                    StatusCode = r.Status?.Code
                })
                .ToList();

            result.Add(new OfficeEngineerPerformanceDto
            {
                EngineerId             = eng.Id,
                NameEn                 = eng.nameEn,
                NameAr                 = eng.nameAr,
                Position               = eng.position,
                DepartmentNameEn       = eng.Department?.nameEn,
                AvgResponseTimeHours   = avgResponse,
                OnTimeDeliveryRate     = onTimeRate,
                OnTimeDeliveries       = onTime,
                TotalDeliveries        = completed.Count,
                DeliveryDateViolations = violations,
                WeeklyActivity         = weekActivity
            });
        }

        return result.OrderBy(e => e.AvgResponseTimeHours).ToList();
    }

    // ── Department Performance Index ──────────────────────────────────────────

    private static DeptPerformanceIndexDto? BuildDeptIndex(
        List<SiteEngineerPerformanceDto>   site,
        List<OfficeEngineerPerformanceDto> office)
    {
        var scores = new List<double>();

        if (site.Count > 0)
        {
            scores.Add(site.Average(e => e.ReportCompletionRate));
            scores.Add(site.Average(e => e.RequestQualityScore));
            // Lower urgent ratio is better — invert it
            scores.Add(100 - site.Average(e => e.UrgentRequestsRatio));
        }

        if (office.Count > 0)
        {
            scores.Add(office.Average(e => e.OnTimeDeliveryRate));
            // Response time: 0 hrs = 100%, 8 hrs = 0%, capped
            scores.Add(office.Average(e => Math.Max(0, 100 - (e.AvgResponseTimeHours / 8.0 * 100))));
            // Violations: 0 = 100%, each violation costs 10 points, floor 0
            scores.Add(office.Average(e => Math.Max(0, 100 - e.DeliveryDateViolations * 10)));
        }

        if (scores.Count == 0) return null;

        double overall = Math.Round(scores.Average(), 1);
        string grade = overall switch
        {
            >= 95 => "A+",
            >= 85 => "A",
            >= 75 => "A-",
            >= 65 => "B+",
            >= 55 => "B",
            >= 45 => "B-",
            _     => "C"
        };

        string benchmark = overall >= 75
            ? $"Performing {Math.Round(overall - 75, 1)}% above quarterly benchmark"
            : $"Performing {Math.Round(75 - overall, 1)}% below quarterly benchmark";

        return new DeptPerformanceIndexDto
        {
            Grade            = grade,
            OverallScore     = overall,
            BenchmarkMessage = benchmark
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static void MarkTopPerformer(List<SiteEngineerPerformanceDto> list)
    {
        if (list.Count == 0) return;
        var best = list.MaxBy(e =>
            e.ReportCompletionRate * 0.4 +
            e.RequestQualityScore  * 0.4 +
            (100 - e.UrgentRequestsRatio) * 0.2);
        if (best != null) best.IsTopPerformer = true;
    }

    private static int CountWorkingDays(DateTime from, DateTime to)
    {
        int days = 0;
        for (var d = from.Date; d <= to.Date; d = d.AddDays(1))
            if (d.DayOfWeek != DayOfWeek.Friday && d.DayOfWeek != DayOfWeek.Saturday)
                days++;
        return days;
    }
}
