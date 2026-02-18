using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestAnalysisDtos;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Timers;

namespace Contracting.Infrustructure.Features.business
{
    public class EngineerRequestAnalysisService : IEngineerRequestAnalysisService
    {
        private readonly ApplicationDbContext _db;

        public EngineerRequestAnalysisService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<SiteEngineerAnalysisDto> GetSiteEngineerAnalysisAsync(CancellationToken cancellationToken = default)
        {
            var currentUserId = CurrentUser.Id;
            if (!currentUserId.HasValue)
                return new SiteEngineerAnalysisDto();

            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId.Value, cancellationToken);

            if (engineer is null)
                return new SiteEngineerAnalysisDto();

            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var items = await QueryRequestsAsync(
                _db.EngineerRequests.AsNoTracking().Where(r => r.EngineerId == engineer.Id),
                statusSets.CompletedStatusIds,
                cancellationToken);

            var totals = CalculateCompletionTotals(items, statusSets.CompletedStatusIds);

            var priorityCounts = items
                .GroupBy(i => new { i.PriorityId, i.PriorityName })
                .Select(g => new PriorityCountDto
                {
                    PriorityId = g.Key.PriorityId,
                    PriorityName = string.IsNullOrWhiteSpace(g.Key.PriorityName) ? "Unspecified" : g.Key.PriorityName,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToList();

            return new SiteEngineerAnalysisDto
            {
                TotalRequests = items.Count,
                CompletedOnTime = totals.CompletedOnTime,
                CompletedOverDeadline = totals.CompletedOverDeadline,
                RequestsByPriority = priorityCounts
            };
        }

        public async Task<OfficeEngineerAnalysisDto> GetOfficeEngineerAnalysisAsync(CancellationToken cancellationToken = default)
        {
            var currentUserId = CurrentUser.Id;
            if (!currentUserId.HasValue)
                return new OfficeEngineerAnalysisDto();

            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId.Value, cancellationToken);

            if (engineer is null)
                return new OfficeEngineerAnalysisDto();

            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var items = await QueryRequestsAsync(
                _db.EngineerRequests.AsNoTracking()
                    .Where(r => r.assignToId == currentUserId.Value || r.assignToId == engineer.Id),
                statusSets.CompletedStatusIds,
                cancellationToken);

            var totals = CalculateCompletionTotals(items, statusSets.CompletedStatusIds);

            var today = DateTime.UtcNow.Date;
            var activeWithinDeadline = items
                .Where(i => !statusSets.CompletedStatusIds.Contains(i.StatusId))
                .Count(i => i.EndDate.HasValue && i.EndDate.Value.Date >= today);

            var percentage = items.Count == 0
                ? 0m
                : Math.Round((decimal)activeWithinDeadline / items.Count * 100m, 2, MidpointRounding.AwayFromZero);

            return new OfficeEngineerAnalysisDto
            {
                TotalRequests = items.Count,
                CompletedOnTime = totals.CompletedOnTime,
                CompletedOverDeadline = totals.CompletedOverDeadline,
                ActiveWithinDeadlineCount = activeWithinDeadline,
                ActiveWithinDeadlinePercentage = percentage
            };
        }

        public async Task<TeamLeadAnalysisDto> GetTeamLeadAnalysisAsync(CancellationToken cancellationToken = default)
        {
            var currentUserId = CurrentUser.Id;
            if (!currentUserId.HasValue)
                return new TeamLeadAnalysisDto();

            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId.Value, cancellationToken);

            if (engineer?.DepartmentId == null)
                return new TeamLeadAnalysisDto();

            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var items = await QueryRequestsAsync(
                _db.EngineerRequests.AsNoTracking().Where(r => r.DepartmentId == engineer.DepartmentId),
                statusSets.CompletedStatusIds,
                cancellationToken);

            var totals = CalculateCompletionTotals(items, statusSets.CompletedStatusIds);

            var onHoldCount = items.Count(i => statusSets.OnHoldStatusIds.Contains(i.StatusId));
            var completedCount = items.Count(i => statusSets.CompletedStatusIds.Contains(i.StatusId));
            var notFinishedCount = items.Count - completedCount - onHoldCount;

            return new TeamLeadAnalysisDto
            {
                TotalRequests = items.Count,
                CompletedOnTime = totals.CompletedOnTime,
                CompletedOverDeadline = totals.CompletedOverDeadline,
                OnHoldCount = onHoldCount,
                NotFinishedCount = notFinishedCount < 0 ? 0 : notFinishedCount
            };
        }



        //هل الطلبات اتقفلت قبل الموعد؟
        //ولا في نفس يوم الموعد؟
        //ولا بعد الموعد(تأخير SLA)؟
        //أي قسم وأي أولوية أكثر التزامًا أو أكثر تأخيرًا؟
        public async Task<SlaBucketsReportDto> GetSlaBucketsByPriorityAndDepartmentAsync(CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            var completedWithDeadline = items
                .Where(i => statusSets.CompletedStatusIds.Contains(i.StatusId))
                .Where(i => i.EndDate.HasValue && i.CompletedAt.HasValue)
                .ToList();

            var buckets = completedWithDeadline
                .GroupBy(i => new { i.DepartmentId, i.DepartmentName, i.PriorityId, i.PriorityName })
                .Select(g =>
                {
                    var early = g.Count(i => i.CompletedAt!.Value.Date < i.EndDate!.Value.Date);
                    var onTime = g.Count(i => i.CompletedAt!.Value.Date == i.EndDate!.Value.Date);
                    var late = g.Count(i => i.CompletedAt!.Value.Date > i.EndDate!.Value.Date);
                    var total = g.Count();
                    return new SlaBucketByPriorityDepartmentDto
                    {
                        DepartmentId = g.Key.DepartmentId,
                        DepartmentName = string.IsNullOrWhiteSpace(g.Key.DepartmentName) ? "Unspecified" : g.Key.DepartmentName,
                        PriorityId = g.Key.PriorityId,
                        PriorityName = string.IsNullOrWhiteSpace(g.Key.PriorityName) ? "Unspecified" : g.Key.PriorityName,
                        CompletedCount = total,
                        EarlyCount = early,
                        OnTimeCount = onTime,
                        LateCount = late,
                        EarlyPercentage = total == 0 ? 0m : Math.Round((decimal)early / total * 100m, 2, MidpointRounding.AwayFromZero),
                        OnTimePercentage = total == 0 ? 0m : Math.Round((decimal)onTime / total * 100m, 2, MidpointRounding.AwayFromZero),
                        LatePercentage = total == 0 ? 0m : Math.Round((decimal)late / total * 100m, 2, MidpointRounding.AwayFromZero)
                    };
                })
                .OrderByDescending(x => x.CompletedCount)
                .ToList();

            return new SlaBucketsReportDto
            {
                TotalCompletedWithDeadline = completedWithDeadline.Count,
                Buckets = buckets
            };
        }

        // هذا  يقيس أعمار الطلبات المفتوحة (Requests)
        public async Task<AgingReportDto> GetAgingReportAsync(CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var now = DateTimeOffset.UtcNow;

            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);
            var openItems = items.Where(i => !statusSets.CompletedStatusIds.Contains(i.StatusId)).ToList();

            var buckets = new List<AgingBucketDto>
            {
                new() { RangeLabel = "0-2 days" },
                new() { RangeLabel = "3-7 days" },
                new() { RangeLabel = "8-14 days" },
                new() { RangeLabel = "15+ days" }
            };

            foreach (var item in openItems)
            {
                var ageDays = (now - item.CreatedDate).TotalDays;
                if (ageDays <= 2)
                    buckets[0].Count++;
                else if (ageDays <= 7)
                    buckets[1].Count++;
                else if (ageDays <= 14)
                    buckets[2].Count++;
                else
                    buckets[3].Count++;
            }

            return new AgingReportDto
            {
                OpenRequests = openItems.Count,
                Buckets = buckets
            };
        }

        // تقرير يقيس متوسط زمن إنجاز الطلبات
        // باستخدام Lead Time و Cycle Time 
        // Lead Time	من إنشاء الطلب حتى إغلاقه
        // Cycle Time  من بدء التنفيذ حتى الإغلاق
        public async Task<LeadCycleTimeDto> GetLeadAndCycleTimeAsync(CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            var completed = items
                .Where(i => statusSets.CompletedStatusIds.Contains(i.StatusId))
                .Where(i => i.CompletedAt.HasValue)
                .ToList();

            var leadTimes = completed
                .Select(i => (i.CompletedAt!.Value - i.CreatedDate).TotalDays)
                .Where(d => d >= 0)
                .ToList();

            var cycleTimes = completed
                .Where(i => i.StartDate.HasValue)
                .Select(i => (i.CompletedAt!.Value - new DateTimeOffset(i.StartDate!.Value)).TotalDays)
                .Where(d => d >= 0)
                .ToList();

            var avgLead = leadTimes.Count == 0 ? 0m : Math.Round((decimal)leadTimes.Average(), 2, MidpointRounding.AwayFromZero);
            var avgCycle = cycleTimes.Count == 0 ? 0m : Math.Round((decimal)cycleTimes.Average(), 2, MidpointRounding.AwayFromZero);

            return new LeadCycleTimeDto
            {
                CompletedRequests = completed.Count,
                AverageLeadTimeDays = avgLead,
                AverageCycleTimeDays = avgCycle
            };
        }

        // تقرير يحدد الطلبات المفتوحة المعرضة للتأخير قريبًا
        // 3 day set at riskLimit
        public async Task<OverdueRiskDto> GetOverdueRiskAsync(CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            var today = DateTime.UtcNow.Date;
            var riskLimit = today.AddDays(3);

            var openWithDeadline = items
                .Where(i => !statusSets.CompletedStatusIds.Contains(i.StatusId))
                .Where(i => i.EndDate.HasValue)
                .ToList();

            var atRisk = openWithDeadline.Count(i => i.EndDate!.Value.Date >= today && i.EndDate!.Value.Date <= riskLimit);

            var percentage = openWithDeadline.Count == 0
                ? 0m
                : Math.Round((decimal)atRisk / openWithDeadline.Count * 100m, 2, MidpointRounding.AwayFromZero);

            return new OverdueRiskDto
            {
                OpenRequestsWithDeadline = openWithDeadline.Count,
                AtRiskCount = atRisk,
                AtRiskPercentage = percentage
            };
        }

        public async Task<AssigneePerformanceReportDto> GetAssigneePerformanceAsync(CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            var assigneeGroups = items
                .Where(i => i.AssignedEngineerId.HasValue)
                .GroupBy(i => new { i.AssignedEngineerId, i.AssignedEngineerName })
                .Select(g =>
                {
                    var total = g.Count();
                    var completed = g.Count(i => statusSets.CompletedStatusIds.Contains(i.StatusId));
                    var completionRate = total == 0 ? 0m : Math.Round((decimal)completed / total * 100m, 2, MidpointRounding.AwayFromZero);

                    var avgCompletionDays = g
                        .Where(i => statusSets.CompletedStatusIds.Contains(i.StatusId) && i.CompletedAt.HasValue)
                        .Select(i =>
                        {
                            var start = i.StartDate.HasValue
                                ? new DateTimeOffset(i.StartDate.Value)
                                : i.CreatedDate;
                            return (i.CompletedAt!.Value - start).TotalDays;
                        })
                        .Where(d => d >= 0)
                        .ToList();

                    var avg = avgCompletionDays.Count == 0
                        ? 0m
                        : Math.Round((decimal)avgCompletionDays.Average(), 2, MidpointRounding.AwayFromZero);
                    var deparmentId = g.Select(s => s.DepartmentId).FirstOrDefault();
                    var deparmentName = g.Select(s => s.DepartmentName).FirstOrDefault();

                    return new AssigneePerformanceDto
                    {
                        EngineerId = g.Key.AssignedEngineerId,
                        EngineerName = string.IsNullOrWhiteSpace(g.Key.AssignedEngineerName) ? "Unspecified" : g.Key.AssignedEngineerName,
                        DepartmentId = deparmentId,
                        DepartmentName = string.IsNullOrEmpty(deparmentName)?"": deparmentName,
                        TotalAssigned = total,
                        CompletedAssigned = completed,
                        CompletionRate = completionRate,
                        AverageCompletionDays = avg
                    };
                })
                .OrderByDescending(x => x.TotalAssigned)
                .ToList();

            return new AssigneePerformanceReportDto
            {
                TotalAssignedRequests = assigneeGroups.Sum(x => x.TotalAssigned),
                Assignees = assigneeGroups
            };
        }

        public async Task<AssigneePerformanceReportDto> GetAssigneePerformanceByEngineerIdAsync(Guid engineerId, CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            var filteredItems = items
                .Where(i => i.AssignedEngineerId.HasValue && i.AssignedEngineerId.Value == engineerId)
                .ToList();

            var assigneeGroups = filteredItems
                .GroupBy(i => new { i.AssignedEngineerId, i.AssignedEngineerName })
                .Select(g =>
                {
                    var total = g.Count();
                    var completed = g.Count(i => statusSets.CompletedStatusIds.Contains(i.StatusId));
                    var completionRate = total == 0 ? 0m : Math.Round((decimal)completed / total * 100m, 2, MidpointRounding.AwayFromZero);

                    var avgCompletionDays = g
                        .Where(i => statusSets.CompletedStatusIds.Contains(i.StatusId) && i.CompletedAt.HasValue)
                        .Select(i =>
                        {
                            var start = i.StartDate.HasValue
                                ? new DateTimeOffset(i.StartDate.Value)
                                : i.CreatedDate;
                            return (i.CompletedAt!.Value - start).TotalDays;
                        })
                        .Where(d => d >= 0)
                        .ToList();

                    var avg = avgCompletionDays.Count == 0
                        ? 0m
                        : Math.Round((decimal)avgCompletionDays.Average(), 2, MidpointRounding.AwayFromZero);
                    var deparmentId = g.Select(s => s.DepartmentId).FirstOrDefault();
                    var deparmentName = g.Select(s => s.DepartmentName).FirstOrDefault();

                    return new AssigneePerformanceDto
                    {
                        EngineerId = g.Key.AssignedEngineerId,
                        EngineerName = string.IsNullOrWhiteSpace(g.Key.AssignedEngineerName) ? "Unspecified" : g.Key.AssignedEngineerName,
                        DepartmentId = deparmentId,
                        DepartmentName = string.IsNullOrEmpty(deparmentName) ? "" : deparmentName,
                        TotalAssigned = total,
                        CompletedAssigned = completed,
                        CompletionRate = completionRate,
                        AverageCompletionDays = avg
                    };
                })
                .OrderByDescending(x => x.TotalAssigned)
                .ToList();

            return new AssigneePerformanceReportDto
            {
                TotalAssignedRequests = assigneeGroups.Sum(x => x.TotalAssigned),
                Assignees = assigneeGroups
            };
        }

        public async Task<ReworkRateDto> GetReworkRateAsync(CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var completedIds = statusSets.CompletedStatusIds.ToList();

            // Get filtered request IDs based on role
            var filteredRequestIds = await ApplyRoleFilter(
                _db.EngineerRequests.AsNoTracking(), filterContext)
                .Select(r => r.Id)
                .ToListAsync(cancellationToken);

            var activities = await _db.EngineerRequestActivites
                .AsNoTracking()
                .Where(a => a.EngineerRequestId.HasValue 
                    && a.StatusId.HasValue
                    && filteredRequestIds.Contains(a.EngineerRequestId.Value))
                .OrderBy(a => a.CreatedDate)
                .Select(a => new { a.EngineerRequestId, a.StatusId, a.CreatedDate })
                .ToListAsync(cancellationToken);

            var grouped = activities
                .GroupBy(a => a.EngineerRequestId!.Value)
                .ToList();

            var requestsWithCompletion = 0;
            var reworked = 0;

            foreach (var group in grouped)
            {
                var ordered = group.OrderBy(a => a.CreatedDate).ToList();
                var seenCompleted = false;
                var hasCompleted = ordered.Any(a => completedIds.Contains(a.StatusId!.Value));
                if (hasCompleted) requestsWithCompletion++;

                foreach (var activity in ordered)
                {
                    if (completedIds.Contains(activity.StatusId!.Value))
                    {
                        seenCompleted = true;
                        continue;
                    }

                    if (seenCompleted)
                    {
                        reworked++;
                        break;
                    }
                }
            }

            var percentage = requestsWithCompletion == 0
                ? 0m
                : Math.Round((decimal)reworked / requestsWithCompletion * 100m, 2, MidpointRounding.AwayFromZero);

            return new ReworkRateDto
            {
                RequestsWithCompletion = requestsWithCompletion,
                ReworkedRequests = reworked,
                ReworkPercentage = percentage
            };
        }

        public async Task<EngineerStatusPercentageReportDto> GetRequestStatusPercentageAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            items = ApplyDateFilter(items, startDate, endDate);

            var engineers = BuildEngineerStatusPercentages(items, statusSets);

            return new EngineerStatusPercentageReportDto
            {
                TotalRequests = items.Count,
                Engineers = engineers
            };
        }

        public async Task<EngineerStatusPercentageReportDto> GetRequestStatusPercentageByEngineerIdAsync(Guid engineerId, int? month = null, int? year = null, CancellationToken cancellationToken = default)
        {
            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            items = ApplyMonthYearFilter(items, month, year);

            var filteredItems = items
                .Where(i => i.AssignedEngineerId.HasValue && i.AssignedEngineerId.Value == engineerId)
                .ToList();

            var engineers = BuildEngineerStatusPercentages(filteredItems, statusSets);

            return new EngineerStatusPercentageReportDto
            {
                TotalRequests = filteredItems.Count,
                Engineers = engineers
            };
        }

        private static List<RequestAnalysisItem> ApplyDateFilter(List<RequestAnalysisItem> items, DateTime? startDate, DateTime? endDate)
        {
            if (startDate.HasValue)
                items = items.Where(i => i.CreatedDate >= new DateTimeOffset(startDate.Value)).ToList();

            if (endDate.HasValue)
                items = items.Where(i => i.CreatedDate <= new DateTimeOffset(endDate.Value.Date.AddDays(1).AddTicks(-1))).ToList();

            return items;
        }

        private static List<RequestAnalysisItem> ApplyMonthYearFilter(List<RequestAnalysisItem> items, int? month, int? year)
        {
            if (year.HasValue)
                items = items.Where(i => i.CreatedDate.Year == year.Value).ToList();

            if (month.HasValue)
                items = items.Where(i => i.CreatedDate.Month == month.Value).ToList();

            return items;
        }

        public async Task<WeeklyCompletionReportDto> GetWeeklyCompletionAsync(int? month = null, int? year = null, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;

            var firstDay = new DateTime(targetYear, targetMonth, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);

            var statusSets = await GetStatusSetsAsync(cancellationToken);
            var filterContext = await GetUserFilterContextAsync(cancellationToken);
            var items = await QueryRequestsDetailedWithFilterAsync(statusSets.CompletedStatusIds, filterContext, cancellationToken);

            // Filter to only completed items within the target month
            var completedItems = items
                .Where(i => statusSets.CompletedStatusIds.Contains(i.StatusId)
                         && i.CompletedAt.HasValue
                         && i.CompletedAt.Value >= new DateTimeOffset(firstDay)
                         && i.CompletedAt.Value < new DateTimeOffset(firstDay.AddMonths(1)))
                .ToList();

            var weeks = new List<WeeklyCompletionDto>();
            var weekNumber = 1;
            var currentStart = firstDay;

            while (currentStart <= lastDay)
            {
                var currentEnd = currentStart.AddDays(6);
                if (currentEnd > lastDay)
                    currentEnd = lastDay;

                var weekStart = new DateTimeOffset(currentStart);
                var weekEnd = new DateTimeOffset(currentEnd.AddDays(1)); // exclusive upper bound

                var weekItems = completedItems
                    .Where(i => i.CompletedAt!.Value >= weekStart && i.CompletedAt!.Value < weekEnd)
                    .ToList();

                var onTime = weekItems.Count(i => i.EndDate.HasValue
                    && i.CompletedAt!.Value <= new DateTimeOffset(i.EndDate.Value));

                var delayed = weekItems.Count(i => i.EndDate.HasValue
                    && i.CompletedAt!.Value > new DateTimeOffset(i.EndDate.Value));

                weeks.Add(new WeeklyCompletionDto
                {
                    WeekNumber = weekNumber,
                    WeekLabel = $"Week {weekNumber}",
                    WeekStartDate = currentStart,
                    WeekEndDate = currentEnd,
                    OnTimeCompletion = onTime,
                    TaskDelayed = delayed
                });

                weekNumber++;
                currentStart = currentEnd.AddDays(1);
            }

            return new WeeklyCompletionReportDto
            {
                Year = targetYear,
                Month = targetMonth,
                TotalOnTime = weeks.Sum(w => w.OnTimeCompletion),
                TotalDelayed = weeks.Sum(w => w.TaskDelayed),
                Weeks = weeks
            };
        }

        private List<EngineerStatusPercentageDto> BuildEngineerStatusPercentages(
            List<RequestAnalysisItem> items,
            (HashSet<Guid> CompletedStatusIds, HashSet<Guid> OnHoldStatusIds) statusSets)
        {
            var today = DateTime.UtcNow.Date;

            return items
                .Where(i => i.AssignedEngineerId.HasValue)
                .GroupBy(i => new { i.AssignedEngineerId, i.AssignedEngineerName })
                .Select(g =>
                {
                    var total = g.Count();

                    // Completed = in a completed status
                    var completed = g.Where(i => statusSets.CompletedStatusIds.Contains(i.StatusId)).ToList();
                    var completedCount = completed.Count;

                    // Finished in time = completed and CompletedAt <= EndDate
                    var finishedInTime = completed
                        .Count(i => i.EndDate.HasValue && i.CompletedAt.HasValue
                                    && i.CompletedAt.Value <= new DateTimeOffset(i.EndDate.Value));

                    // On hold = status is on hold
                    var onHold = g.Count(i => statusSets.OnHoldStatusIds.Contains(i.StatusId));

                    // Delayed = completed late (CompletedAt > EndDate) OR open and past deadline
                    var completedLate = completed
                        .Count(i => i.EndDate.HasValue && i.CompletedAt.HasValue
                                    && i.CompletedAt.Value > new DateTimeOffset(i.EndDate.Value));

                    var openOverdue = g
                        .Where(i => !statusSets.CompletedStatusIds.Contains(i.StatusId)
                                    && !statusSets.OnHoldStatusIds.Contains(i.StatusId))
                        .Count(i => i.EndDate.HasValue && i.EndDate.Value.Date < today);

                    var delayed = completedLate + openOverdue;

                    return new EngineerStatusPercentageDto
                    {
                        EngineerId = g.Key.AssignedEngineerId,
                        EngineerName = string.IsNullOrWhiteSpace(g.Key.AssignedEngineerName) ? "Unspecified" : g.Key.AssignedEngineerName,
                        TotalRequests = total,
                        FinishedInTimeCount = finishedInTime,
                        FinishedInTimePercentage = total == 0 ? 0m : Math.Round((decimal)finishedInTime / total * 100m, 2, MidpointRounding.AwayFromZero),
                        OnHoldCount = onHold,
                        OnHoldPercentage = total == 0 ? 0m : Math.Round((decimal)onHold / total * 100m, 2, MidpointRounding.AwayFromZero),
                        DelayedCount = delayed,
                        DelayedPercentage = total == 0 ? 0m : Math.Round((decimal)delayed / total * 100m, 2, MidpointRounding.AwayFromZero),
                        CompletedCount = completedCount,
                        CompletedPercentage = total == 0 ? 0m : Math.Round((decimal)completedCount / total * 100m, 2, MidpointRounding.AwayFromZero)
                    };
                })
                .OrderByDescending(x => x.TotalRequests)
                .ToList();
        }

        private sealed record RequestAnalysisItem(
            Guid Id,
            Guid StatusId,
            Guid? PriorityId,
            string? PriorityName,
            Guid? DepartmentId,
            string? DepartmentName,
            DateTime? EndDate,
            DateTime? StartDate,
            DateTimeOffset CreatedDate,
            Guid? AssignedEngineerId,
            string? AssignedEngineerName,
            DateTimeOffset? CompletedAt);

        private async Task<List<RequestAnalysisItem>> QueryRequestsAsync(
            IQueryable<EngineerRequest> baseQuery,
            HashSet<Guid> completedStatusIds,
            CancellationToken cancellationToken)
        {
            var completedIds = completedStatusIds.ToList();

            return await baseQuery
                .Select(r => new RequestAnalysisItem(
                    r.Id,
                    r.StatusId,
                    r.PriorityId,
                    r.Priority != null ? r.Priority.nameEn : null,
                    r.DepartmentId,
                    r.Department != null ? r.Department.nameEn : null,
                    r.endDate,
                    r.startDate,
                    r.CreatedDate,
                    r.assignToId ?? r.EngineerId,
                    r.assignTo != null
                        ? r.assignTo.nameEn
                        : r.Engineer != null ? r.Engineer.nameEn : null,
                    completedIds.Count == 0
                        ? null
                        : _db.EngineerRequestActivites
                            .Where(a => a.EngineerRequestId == r.Id
                                && a.StatusId.HasValue
                                && completedIds.Contains(a.StatusId.Value))
                            .OrderByDescending(a => a.CreatedDate)
                            .Select(a => (DateTimeOffset?)a.CreatedDate)
                            .FirstOrDefault()
                ))
                .ToListAsync(cancellationToken);
        }

        private async Task<List<RequestAnalysisItem>> QueryRequestsDetailedAsync(
            HashSet<Guid> completedStatusIds,
            CancellationToken cancellationToken)
        {
            return await QueryRequestsAsync(
                _db.EngineerRequests
                    .AsNoTracking()
                    .Include(r => r.Priority)
                    .Include(r => r.Department)
                    .Include(r => r.Engineer)
                    .Include(r => r.assignTo),
                completedStatusIds,
                cancellationToken);
        }

        private static (int CompletedOnTime, int CompletedOverDeadline) CalculateCompletionTotals(
            List<RequestAnalysisItem> items,
            HashSet<Guid> completedStatusIds)
        {
            var completedOnTime = 0;
            var completedOverDeadline = 0;

            foreach (var item in items)
            {
                if (!completedStatusIds.Contains(item.StatusId))
                    continue;

                if (!item.EndDate.HasValue || !item.CompletedAt.HasValue)
                    continue;

                var endDate = new DateTimeOffset(item.EndDate.Value);
                if (item.CompletedAt.Value <= endDate)
                    completedOnTime++;
                else
                    completedOverDeadline++;
            }

            return (completedOnTime, completedOverDeadline);
        }

        private async Task<(HashSet<Guid> CompletedStatusIds, HashSet<Guid> OnHoldStatusIds)> GetStatusSetsAsync(CancellationToken cancellationToken)
        {
            var statuses = await _db.Statuses.AsNoTracking().ToListAsync(cancellationToken);

            var completedKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "completed", "complete", "done", "finished", "finish", "closed"
            };

            var onHoldKeywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "on hold", "hold", "paused", "pause", "suspend", "suspended"
            };

            bool HasKeyword(string? value, HashSet<string> keywords)
            {
                if (string.IsNullOrWhiteSpace(value)) return false;
                var normalized = value.Trim().ToLowerInvariant();
                return keywords.Any(k => normalized.Contains(k));
            }

            var completedIds = statuses
                .Where(s => HasKeyword(s.Code, completedKeywords)
                         || HasKeyword(s.nameEn, completedKeywords)
                         || HasKeyword(s.nameAr, completedKeywords))
                .Select(s => s.Id)
                .ToHashSet();

            var onHoldIds = statuses
                .Where(s => HasKeyword(s.Code, onHoldKeywords)
                         || HasKeyword(s.nameEn, onHoldKeywords)
                         || HasKeyword(s.nameAr, onHoldKeywords))
                .Select(s => s.Id)
                .ToHashSet();

            return (completedIds, onHoldIds);
        }

        private sealed record UserFilterContext(
            bool IsAdmin,
            bool IsTeamLead,
            bool IsOfficeEngineer,
            Guid? UserId,
            Guid? EngineerId,
            Guid? DepartmentId);

        private async Task<UserFilterContext> GetUserFilterContextAsync(CancellationToken cancellationToken)
        {
            var roles = CurrentUser.Roles;
            var isAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                      || r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));
            var isTeamLead = roles.Any(r => r.Equals(RoleNames.Teamleadengineer, StringComparison.OrdinalIgnoreCase));
            var isOfficeEngineer = roles.Any(r => r.Equals(RoleNames.Officeengineer, StringComparison.OrdinalIgnoreCase));

            var currentUserId = CurrentUser.Id;
            Guid? engineerId = null;
            Guid? departmentId = null;

            if (currentUserId.HasValue && !isAdmin)
            {
                var engineer = await _db.Engineers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ApplicationUserId == currentUserId.Value, cancellationToken);

                if (engineer != null)
                {
                    engineerId = engineer.Id;
                    departmentId = engineer.DepartmentId;
                }
            }

            return new UserFilterContext(isAdmin, isTeamLead, isOfficeEngineer, currentUserId, engineerId, departmentId);
        }

        private IQueryable<EngineerRequest> ApplyRoleFilter(IQueryable<EngineerRequest> query, UserFilterContext context)
        {
            // Admin/SuperAdmin: No filter - see all
            if (context.IsAdmin)
                return query;

            // TeamLead: Filter by department
            if (context.IsTeamLead && context.DepartmentId.HasValue)
                return query.Where(r => r.DepartmentId == context.DepartmentId.Value);

            // Office-engineer: Filter by assigned to them or they created
            if (context.IsOfficeEngineer)
            {
                var userId = context.UserId;
                var engineerId = context.EngineerId;
                return query.Where(r => r.assignToId == userId 
                                     || r.assignToId == engineerId
                                     || r.EngineerId == engineerId);
            }

            // Default: No access (return empty)
            return query.Where(r => false);
        }

        private async Task<List<RequestAnalysisItem>> QueryRequestsDetailedWithFilterAsync(
            HashSet<Guid> completedStatusIds,
            UserFilterContext filterContext,
            CancellationToken cancellationToken)
        {
            var baseQuery = _db.EngineerRequests
                .AsNoTracking()
                .Include(r => r.Priority)
                .Include(r => r.Department)
                .Include(r => r.Engineer)
                .Include(r => r.assignTo);

            var filteredQuery = ApplyRoleFilter(baseQuery, filterContext);

            return await QueryRequestsAsync(filteredQuery, completedStatusIds, cancellationToken);
        }
    }
}
