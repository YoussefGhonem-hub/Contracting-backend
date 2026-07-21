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

            // Engineers can belong to multiple departments via EngineerDepartments, in addition to
            // (or instead of) the legacy single Engineer.DepartmentId — both must be checked, or an
            // engineer whose relevant membership is only in EngineerDepartments gets silently dropped.
            if (filter.BranchId.HasValue)
            {
                var branchId = filter.BranchId.Value;
                query = query.Where(e =>
                    (e.Department != null && e.Department.BranchId == branchId)
                    || e.EngineerDepartments.Any(ed => ed.Department != null && ed.Department.BranchId == branchId));
            }

            string? deptName = null;
            if (filter.DepartmentId.HasValue)
            {
                var deptId = filter.DepartmentId.Value;
                query    = query.Where(e =>
                    e.DepartmentId == deptId
                    || e.EngineerDepartments.Any(ed => ed.DepartmentId == deptId));
                deptName = await _db.Departmentes
                    .Where(d => d.Id == deptId)
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

            var leadEngineer = await _db.Engineers
                .FirstOrDefaultAsync(e => e.ApplicationUserId == userId && !e.IsDeleted, ct);
            if (leadEngineer is null) return (null, null);

            // A team lead can lead more than one department via EngineerDepartments — the legacy
            // single Engineer.DepartmentId is only a fallback for engineers never migrated to it.
            var leadDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == leadEngineer.Id && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
                .Select(ed => ed.DepartmentId)
                .ToListAsync(ct);

            if (!leadDeptIds.Any() && leadEngineer.DepartmentId.HasValue)
                leadDeptIds.Add(leadEngineer.DepartmentId.Value);

            if (!leadDeptIds.Any()) return (new List<Guid>(), null);

            // Narrow to one of the lead's own departments if requested
            if (filter.DepartmentId.HasValue)
                leadDeptIds = leadDeptIds.Where(d => d == filter.DepartmentId.Value).ToList();

            var deptName = leadDeptIds.Count == 1
                ? await _db.Departmentes
                    .Where(d => d.Id == leadDeptIds[0])
                    .Select(d => d.nameEn)
                    .FirstOrDefaultAsync(ct)
                : null;

            var query = _db.Engineers
                .Where(e => !e.IsDeleted
                    && (leadDeptIds.Contains(e.DepartmentId ?? Guid.Empty)
                        || e.EngineerDepartments.Any(ed => leadDeptIds.Contains(ed.DepartmentId))));

            // Team lead can further scope to a specific engineer in their dept(s)
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

        // Bulk-load project counts per engineer (one report expected per project per working day)
        var projectCountsRaw = await _db.EngineerProjects
            .Where(ep => engineerIds.Contains(ep.EngineerId) && !ep.IsDeleted)
            .GroupBy(ep => ep.EngineerId)
            .Select(g => new { EngineerId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var projectCountDict = projectCountsRaw.ToDictionary(x => x.EngineerId, x => x.Count);

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
            .Include(r => r.EngineerRequestActivites.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Status)
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

            // Report Completion — engineer must submit one report per project per working day
            int projectCount    = projectCountDict.TryGetValue(eng.Id, out var pc) ? Math.Max(pc, 1) : 1;
            int expectedReports = workingDays * projectCount;
            int submitted       = myReports.Count;
            double completionRate = expectedReports > 0
                ? Math.Round((double)submitted / expectedReports * 100, 1)
                : 0;

            // High-Priority Requests Ratio (system uses High / Medium / Low)
            int total  = myRequests.Count;
            int urgent = myRequests.Count(r =>
                r.Priority != null &&
                (r.Priority.nameEn?.Contains("high",   StringComparison.OrdinalIgnoreCase) == true
              || r.Priority.code?.Contains("high",     StringComparison.OrdinalIgnoreCase) == true));
            double urgentRatio = total > 0
                ? Math.Round((double)urgent / total * 100, 1)
                : 0;

            // Missing Information — requests that ever had a "Missing Information" activity
            // (covers both currently in that status AND those that recovered and moved on)
            int missingInfo = myRequests.Count(r =>
                r.EngineerRequestActivites.Any(a =>
                    a.Status?.Code != null &&
                    a.Status.Code.Equals(MasterStatusCodes.MissingInformation, StringComparison.OrdinalIgnoreCase)));
            double missingInfoRatio = total > 0
                ? Math.Round((double)missingInfo / total * 100, 1)
                : 0;

            // Request Quality Score — of requests that reached a final outcome
            // (Completed or Rejected), what percentage were Completed?
            //
            // Previous logic checked "completed AND never had a Rejected activity",
            // but a request can never move from Rejected back to any other status
            // (hard-blocked in TakeActionOnRequestAsync/UpdateEngineerRequestAsync -
            // confirmed against production data: 0 of 18 Completed requests have
            // ever had a Rejected activity). That made the old check always true by
            // construction, so the score was structurally stuck at 100% for every
            // engineer with any completed work - it never actually measured quality.
            var completed = myRequests
                .Where(r => r.Status?.Code == MasterStatusCodes.Completed)
                .ToList();

            int rejectedCount = myRequests.Count(r => r.Status?.Code == MasterStatusCodes.Rejected);
            int finalizedCount = completed.Count + rejectedCount;

            double qualityScore = finalizedCount > 0
                ? Math.Round((double)completed.Count / finalizedCount * 100, 1)
                : 100;

            result.Add(new SiteEngineerPerformanceDto
            {
                EngineerId             = eng.Id,
                NameEn                 = eng.nameEn,
                NameAr                 = eng.nameAr,
                Position               = eng.position,
                DepartmentNameEn       = eng.Department?.nameEn,
                ReportCompletionRate   = completionRate,
                ReportsSubmitted       = submitted,
                WorkingDaysInPeriod    = workingDays,
                ProjectCount           = projectCount,
                ExpectedReportsCount   = expectedReports,
                UrgentRequestsRatio    = urgentRatio,
                UrgentRequests         = urgent,
                TotalRequests          = total,
                MissingInfoRequests    = missingInfo,
                MissingInfoRatio       = missingInfoRatio,
                RequestQualityScore    = qualityScore,
                AcceptedOnFirstTry     = completed.Count,
                TotalCompletedRequests = completed.Count,
                RejectedRequests       = rejectedCount,
                TotalFinalizedRequests = finalizedCount
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

            // On-Time Delivery — use the date of the Completed status activity,
            // falling back to ModifiedDate only if no such activity exists.
            var completed = myRequests
                .Where(r => r.Status?.Code == MasterStatusCodes.Completed)
                .ToList();

            int onTime = completed.Count(r =>
            {
                if (!r.endDate.HasValue) return false;
                var completedAt = r.EngineerRequestActivites
                    .Where(a => a.Status?.Code == MasterStatusCodes.Completed)
                    .OrderByDescending(a => a.CreatedDate)
                    .Select(a => (DateTimeOffset?)a.CreatedDate)
                    .FirstOrDefault()
                    ?? r.ModifiedDate
                    ?? r.CreatedDate;
                return completedAt.UtcDateTime <= r.endDate.Value.ToUniversalTime();
            });

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
                TotalRequests          = myRequests.Count,
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

        // ReportCompletionRate/RequestQualityScore/UrgentRequestsRatio (and their office-side
        // equivalents below) default to their best possible value for an engineer with zero
        // activity in the period - averaging those in would let idle staff inflate the index as
        // if they performed flawlessly. Prefer the activity-only subset; fall back to everyone
        // only if nobody had any activity at all (so the index still returns a value).
        var activeSite = site.Where(e => e.ReportsSubmitted > 0 || e.TotalRequests > 0).ToList();
        var siteForScoring = activeSite.Count > 0 ? activeSite : site;

        if (siteForScoring.Count > 0)
        {
            scores.Add(siteForScoring.Average(e => e.ReportCompletionRate));
            scores.Add(siteForScoring.Average(e => e.RequestQualityScore));
            // Lower urgent ratio is better — invert it
            scores.Add(100 - siteForScoring.Average(e => e.UrgentRequestsRatio));
        }

        var activeOffice = office.Where(e => e.TotalRequests > 0).ToList();
        var officeForScoring = activeOffice.Count > 0 ? activeOffice : office;

        if (officeForScoring.Count > 0)
        {
            scores.Add(officeForScoring.Average(e => e.OnTimeDeliveryRate));
            // Response time: 0 hrs = 100%, 8 hrs = 0%, capped
            scores.Add(officeForScoring.Average(e => Math.Max(0, 100 - (e.AvgResponseTimeHours / 8.0 * 100))));
            // Violations: 0 = 100%, each violation costs 10 points, floor 0
            scores.Add(officeForScoring.Average(e => Math.Max(0, 100 - e.DeliveryDateViolations * 10)));
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

        // RequestQualityScore defaults to 100 and UrgentRequestsRatio defaults to 0 when an
        // engineer has zero completed/total requests in the period - i.e. an engineer who did
        // nothing scores as if they were flawless. Only rank engineers who actually submitted a
        // report or had at least one request, so idle staff can't outrank real (if imperfect) work.
        var candidates = list.Where(e => e.ReportsSubmitted > 0 || e.TotalRequests > 0).ToList();
        if (candidates.Count == 0) return;

        var best = candidates.MaxBy(e =>
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

    // ── Full Report ───────────────────────────────────────────────────────────────

    public async Task<ErrorOr<FullPerformanceReportDto>> GetFullReportAsync(
        PerformanceAnalyticsFilterDto filter, CancellationToken ct = default)
    {
        var from = filter.FromDate ?? DateTime.UtcNow.AddMonths(-1).Date;
        var to   = (filter.ToDate  ?? DateTime.UtcNow).Date.AddDays(1);

        var (allowedIds, deptName) = await ResolveAllowedEngineersAsync(filter, ct);
        if (allowedIds is null)
            return Error.Forbidden("Performance.Forbidden", "You do not have access to this data.");

        var siteIds   = await FilterByRoleAsync(allowedIds, RoleNames.Siteengineer,   ct);
        var officeIds = await FilterByRoleAsync(allowedIds, RoleNames.Officeengineer, ct);

        // Reuse base metrics
        var siteBase   = await BuildSiteMetricsAsync(siteIds,   from, to, ct);
        var officeBase = await BuildOfficeMetricsAsync(officeIds, from, to, ct);
        MarkTopPerformer(siteBase);

        // ── Site engineers: enrich with priority breakdown ────────────────────
        var siteRequests = await _db.EngineerRequests
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.EngineerRequestActivites.Where(a => !a.IsDeleted))
                .ThenInclude(a => a.Status)
            .Where(r => r.EngineerId != null
                     && siteIds.Contains(r.EngineerId!.Value)
                     && r.CreatedDate >= from && r.CreatedDate < to
                     && !r.IsDeleted)
            .ToListAsync(ct);

        var siteFullList = siteBase.Select(dto =>
        {
            var mine = siteRequests.Where(r => r.EngineerId == dto.EngineerId).ToList();
            int high   = mine.Count(r => r.Priority?.nameEn?.Contains("high",   StringComparison.OrdinalIgnoreCase) == true || r.Priority?.code?.Contains("high", StringComparison.OrdinalIgnoreCase) == true);
            int medium = mine.Count(r => r.Priority?.nameEn?.Contains("medium", StringComparison.OrdinalIgnoreCase) == true || r.Priority?.code?.Contains("medium", StringComparison.OrdinalIgnoreCase) == true);
            int low    = mine.Count(r => r.Priority?.nameEn?.Contains("low",    StringComparison.OrdinalIgnoreCase) == true || r.Priority?.code?.Contains("low", StringComparison.OrdinalIgnoreCase) == true);
            double composite = Math.Round(dto.ReportCompletionRate * 0.4 + dto.RequestQualityScore * 0.4 + (100 - dto.UrgentRequestsRatio) * 0.2, 1);

            return new SiteEngineerFullDto
            {
                EngineerId             = dto.EngineerId,
                NameEn                 = dto.NameEn,
                NameAr                 = dto.NameAr,
                Position               = dto.Position,
                DepartmentNameEn       = dto.DepartmentNameEn,
                IsTopPerformer         = dto.IsTopPerformer,
                ReportCompletionRate   = dto.ReportCompletionRate,
                ReportsSubmitted       = dto.ReportsSubmitted,
                WorkingDaysInPeriod    = dto.WorkingDaysInPeriod,
                UrgentRequestsRatio    = dto.UrgentRequestsRatio,
                UrgentRequests         = dto.UrgentRequests,
                TotalRequests          = dto.TotalRequests,
                RequestQualityScore    = dto.RequestQualityScore,
                AcceptedOnFirstTry     = dto.AcceptedOnFirstTry,
                TotalCompletedRequests = dto.TotalCompletedRequests,
                RejectedRequests       = dto.RejectedRequests,
                TotalFinalizedRequests = dto.TotalFinalizedRequests,
                HighPriorityRequests   = high,
                MediumPriorityRequests = medium,
                LowPriorityRequests    = low,
                CompositeScore         = composite
            };
        }).OrderByDescending(e => e.CompositeScore).ToList();

        // ── Office engineers: enrich ──────────────────────────────────────────
        var officeRequests = await _db.EngineerRequests
            .Include(r => r.Status)
            .Where(r => r.assignToId != null
                     && officeIds.Contains(r.assignToId!.Value)
                     && r.CreatedDate >= from && r.CreatedDate < to
                     && !r.IsDeleted)
            .ToListAsync(ct);

        var officeFullList = officeBase.Select(dto =>
        {
            var mine = officeRequests.Where(r => r.assignToId == dto.EngineerId).ToList();
            int inProg  = mine.Count(r => r.Status?.Code != MasterStatusCodes.Completed && r.Status?.Code != MasterStatusCodes.Rejected);
            int pending = mine.Count(r => r.Status?.Code == null);
            double composite = Math.Round(
                dto.OnTimeDeliveryRate * 0.5 +
                Math.Max(0, 100 - dto.AvgResponseTimeHours / 8.0 * 100) * 0.35 +
                Math.Max(0, 100 - dto.DeliveryDateViolations * 10) * 0.15, 1);

            return new OfficeEngineerFullDto
            {
                EngineerId             = dto.EngineerId,
                NameEn                 = dto.NameEn,
                NameAr                 = dto.NameAr,
                Position               = dto.Position,
                DepartmentNameEn       = dto.DepartmentNameEn,
                AvgResponseTimeHours   = dto.AvgResponseTimeHours,
                OnTimeDeliveryRate     = dto.OnTimeDeliveryRate,
                OnTimeDeliveries       = dto.OnTimeDeliveries,
                TotalDeliveries        = dto.TotalDeliveries,
                DeliveryDateViolations = dto.DeliveryDateViolations,
                WeeklyActivity         = dto.WeeklyActivity,
                PendingRequests        = pending,
                InProgressCount        = inProg,
                CompositeScore         = composite
            };
        }).OrderByDescending(e => e.CompositeScore).ToList();

        // ── Summary ───────────────────────────────────────────────────────────
        int totalReq  = siteFullList.Sum(e => e.TotalRequests);
        int totalComp = siteFullList.Sum(e => e.TotalCompletedRequests);
        int totalHigh = siteFullList.Sum(e => e.HighPriorityRequests);
        int totalReps = siteFullList.Sum(e => e.ReportsSubmitted);
        var topPerf   = siteFullList.FirstOrDefault(e => e.IsTopPerformer);

        double avgCompletion = siteFullList.Count > 0 ? siteFullList.Average(e => e.ReportCompletionRate) : 0;
        double avgQuality    = siteFullList.Count > 0 ? siteFullList.Average(e => e.RequestQualityScore)  : 0;
        double avgOnTime     = officeFullList.Count > 0 ? officeFullList.Average(e => e.OnTimeDeliveryRate) : 0;

        var summary = new FullReportSummaryDto
        {
            TotalSiteEngineers    = siteFullList.Count,
            TotalOfficeEngineers  = officeFullList.Count,
            TotalRequests         = totalReq,
            TotalCompleted        = totalComp,
            TotalHighPriority     = totalHigh,
            TotalReportsSubmitted = totalReps,
            OverallCompletionRate = Math.Round(avgCompletion, 1),
            OverallQualityScore   = Math.Round(avgQuality, 1),
            OverallOnTimeRate     = Math.Round(avgOnTime, 1),
            TopPerformerNameEn    = topPerf?.NameEn,
            TopPerformerNameAr    = topPerf?.NameAr,
        };

        // ── Score Breakdown ───────────────────────────────────────────────────
        double highPrioAvg  = siteFullList.Count > 0 ? siteFullList.Average(e => e.UrgentRequestsRatio) : 0;
        double responseAvg  = officeFullList.Count > 0 ? officeFullList.Average(e => e.AvgResponseTimeHours) : 0;
        double violAvg      = officeFullList.Count > 0 ? officeFullList.Average(e => e.DeliveryDateViolations) : 0;

        var scoreBreakdown = new ScoreBreakdownDto
        {
            ReportCompletionScore = Math.Round(avgCompletion, 1),
            RequestQualityScore   = Math.Round(avgQuality,    1),
            HighPriorityScore     = Math.Round(100 - highPrioAvg, 1),
            OnTimeDeliveryScore   = Math.Round(avgOnTime,     1),
            ResponseTimeScore     = Math.Round(Math.Max(0, 100 - responseAvg / 8.0 * 100), 1),
            ViolationsScore       = Math.Round(Math.Max(0, 100 - violAvg * 10), 1)
        };

        // ── Insights ──────────────────────────────────────────────────────────
        var insights = new List<ReportInsightDto>();

        if (avgCompletion < 70)
            insights.Add(new ReportInsightDto { Type = "warning", Message = $"Report completion rate is {avgCompletion:F0}% — below the 70% target. Encourage site engineers to submit daily reports consistently." });
        else if (avgCompletion >= 90)
            insights.Add(new ReportInsightDto { Type = "success", Message = $"Excellent report completion rate of {avgCompletion:F0}%. Keep up the consistent reporting." });

        if (avgQuality < 80)
            insights.Add(new ReportInsightDto { Type = "warning", Message = $"Average request quality score is {avgQuality:F0}% — requests are being rejected before completion. Review request clarity guidelines." });
        else if (avgQuality >= 95)
            insights.Add(new ReportInsightDto { Type = "success", Message = $"Outstanding quality score of {avgQuality:F0}% — most requests are accepted without rejection." });

        if (highPrioAvg > 40)
            insights.Add(new ReportInsightDto { Type = "warning", Message = $"{highPrioAvg:F0}% of requests are high-priority — team may be under pressure. Consider workload balancing." });

        if (avgOnTime < 70 && officeFullList.Count > 0)
            insights.Add(new ReportInsightDto { Type = "warning", Message = $"On-time delivery rate is {avgOnTime:F0}% — below target. Office engineers should review deadline management." });
        else if (avgOnTime >= 90 && officeFullList.Count > 0)
            insights.Add(new ReportInsightDto { Type = "success", Message = $"On-time delivery rate of {avgOnTime:F0}% — excellent delivery performance." });

        if (responseAvg > 8 && officeFullList.Count > 0)
            insights.Add(new ReportInsightDto { Type = "warning", Message = $"Average response time is {responseAvg:F1}h — exceeds the 8-hour target. Prioritize faster initial responses." });

        if (insights.Count == 0)
            insights.Add(new ReportInsightDto { Type = "info", Message = "Overall performance is within acceptable ranges. Continue monitoring key metrics." });

        var deptIndex = BuildDeptIndex(siteBase, officeBase);

        return new FullPerformanceReportDto
        {
            DepartmentNameEn = deptName,
            GeneratedAt      = DateTime.UtcNow,
            FromDate         = from,
            ToDate           = to.AddDays(-1),
            DeptIndex        = deptIndex,
            ScoreBreakdown   = scoreBreakdown,
            Summary          = summary,
            SiteEngineers    = siteFullList,
            OfficeEngineers  = officeFullList,
            Insights         = insights
        };
    }
}
