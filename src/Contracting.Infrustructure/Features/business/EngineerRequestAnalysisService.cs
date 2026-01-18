using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.BusinessDtos.EngineerRequestAnalysisDtos;
using Microsoft.EntityFrameworkCore;

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

        private sealed record RequestAnalysisItem(
            Guid Id,
            Guid StatusId,
            Guid? PriorityId,
            string? PriorityName,
            DateTime? EndDate,
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
                    r.endDate,
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
    }
}
