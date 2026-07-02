using Contracting.Domain.Entities.business;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerSiteReportDto;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.business
{
    public class EngineerSiteReportService : IEngineerSiteReportService
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public EngineerSiteReportService(ApplicationDbContext db, IMapper mapper, IStorageService storageService)
        {
            _db = db;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetEngineerSiteReportDto> CreateEngineerSiteReportAsync(CreateEngineerSiteReportDto dto)
        {
            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

            if (engineer is null)
                return null!;

            var report = new EngineerSiteReport
            {
                EngineerId         = engineer.Id,
                ProjectId          = dto.ProjectId == Guid.Empty ? null : dto.ProjectId,
                ReportDate         = dto.ReportDate ?? Contracting.Shared.Common.DateTimeHelper.Now,
                NoWorkToday        = dto.NoWorkToday,
                // When NoWorkToday is true, work-detail fields are intentionally left null/empty
                WorkPerformedToday = dto.NoWorkToday ? null : dto.WorkPerformedToday,
                MaterialDetails    = dto.NoWorkToday ? null : dto.MaterialDetails,
                IssuesOrDelays     = dto.NoWorkToday ? null : dto.IssuesOrDelays,
                ClientVisitToday   = dto.NoWorkToday ? false : dto.ClientVisitToday,
                VisitDetails       = dto.NoWorkToday ? null : dto.VisitDetails,
                Workers            = new List<ReportConstructionItemWorker>(),
                Attachments        = new List<EngineerSiteReportAttachment>()
            };

            // Workers by Construction Item — skip when NoWorkToday is set
            if (!dto.NoWorkToday && dto.Workers != null && dto.Workers.Any())
            {
                foreach (var w in dto.Workers)
                {
                    report.Workers.Add(new ReportConstructionItemWorker
                    {
                        ConstructionItemId = w.ConstructionItemId,
                        Count = w.Count
                    });
                }
            }

            // 9. Attachments
            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                {
                    report.Attachments = uploaded.Select(f => new EngineerSiteReportAttachment
                    {
                        Key = f.Key,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        FileSize = f.FileSize,
                        Url = f.Url
                    }).ToList();
                }
            }

            await _db.EngineerSiteReports.AddAsync(report);
            await _db.SaveChangesAsync();

            return await GetEngineerSiteReportByIdAsync(report.Id);
        }

        public async Task<GetEngineerSiteReportDto> GetEngineerSiteReportByIdAsync(Guid reportId)
        {
            var report = await BuildReportQuery()
                .FirstOrDefaultAsync(r => r.Id == reportId);

            return report is null ? null! : _mapper.Map<GetEngineerSiteReportDto>(report);
        }

        public async Task<PaginatedList<GetEngineerSiteReportDto>> GetMyEngineerSiteReportsAsync(EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default)
        {
            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId), cancellationToken);

            if (engineer is null)
            {
                return new PaginatedList<GetEngineerSiteReportDto>(
                    new List<GetEngineerSiteReportDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            // Get project IDs this engineer is assigned to
            var assignedProjectIds = await _db.EngineerProjects
                .AsNoTracking()
                .Where(ep => ep.EngineerId == engineer.Id)
                .Select(ep => ep.ProjectId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Get project IDs this engineer has reported on
            var reportedProjectIds = await _db.EngineerSiteReports
                .AsNoTracking()
                .Where(r => r.EngineerId == engineer.Id && r.ProjectId != null)
                .Select(r => r.ProjectId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Combine both: assigned projects + reported projects
            var allProjectIds = assignedProjectIds.Union(reportedProjectIds).Distinct().ToList();

            // Return all reports for those projects (by any engineer)
            var query = BuildReportQuery()
                .Where(r => r.ProjectId != null && allProjectIds.Contains(r.ProjectId!.Value));

            query = ApplyDateRangeFilter(query, filter);

            if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ProjectId == filter.ProjectId.Value);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.ReportDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            if (totalCount == 0)
            {
                return new PaginatedList<GetEngineerSiteReportDto>(
                    new List<GetEngineerSiteReportDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var reports = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var reportDtos = _mapper.Map<List<GetEngineerSiteReportDto>>(reports);

            return new PaginatedList<GetEngineerSiteReportDto>(
                reportDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<PaginatedList<GetEngineerSiteReportDto>> GetEngineerSiteReportsByEngineerIdAsync(Guid engineerId, EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default)
        {
            // Get project IDs this engineer is assigned to
            var assignedProjectIds = await _db.EngineerProjects
                .AsNoTracking()
                .Where(ep => ep.EngineerId == engineerId)
                .Select(ep => ep.ProjectId)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Get project IDs this engineer has reported on
            var reportedProjectIds = await _db.EngineerSiteReports
                .AsNoTracking()
                .Where(r => r.EngineerId == engineerId && r.ProjectId != null)
                .Select(r => r.ProjectId!.Value)
                .Distinct()
                .ToListAsync(cancellationToken);

            // Combine both: assigned projects + reported projects
            var allProjectIds = assignedProjectIds.Union(reportedProjectIds).Distinct().ToList();

            // Return all reports for those projects (by any engineer)
            var query = BuildReportQuery()
                .Where(r => r.ProjectId != null && allProjectIds.Contains(r.ProjectId!.Value));

            query = ApplyDateRangeFilter(query, filter);

            if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ProjectId == filter.ProjectId.Value);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.ReportDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            if (totalCount == 0)
            {
                return new PaginatedList<GetEngineerSiteReportDto>(
                    new List<GetEngineerSiteReportDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var reports = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var reportDtos = _mapper.Map<List<GetEngineerSiteReportDto>>(reports);

            return new PaginatedList<GetEngineerSiteReportDto>(
                reportDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        public async Task<PaginatedList<GetEngineerSiteReportDto>> GetAllEngineerSiteReportsAsync(EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = BuildReportQuery();

            if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            {
                query = query.Where(r => r.ProjectId == filter.ProjectId.Value);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.ReportDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            if (totalCount == 0)
            {
                return new PaginatedList<GetEngineerSiteReportDto>(
                    new List<GetEngineerSiteReportDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var reports = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var reportDtos = _mapper.Map<List<GetEngineerSiteReportDto>>(reports);

            return new PaginatedList<GetEngineerSiteReportDto>(
                reportDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }

        private IQueryable<EngineerSiteReport> BuildReportQuery()
        {
            return _db.EngineerSiteReports
                .Include(r => r.Project)
                    .ThenInclude(p => p.Branch)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Workers)
                    .ThenInclude(w => w.ConstructionItem)
                .Include(r => r.Attachments)
                .AsNoTracking();
        }

        private static IQueryable<EngineerSiteReport> ApplyDateRangeFilter(
            IQueryable<EngineerSiteReport> query,
            EngineerSiteReportFilterDto filter)
        {
            if (filter.FromDate.HasValue)
            {
                var from = new DateTimeOffset(filter.FromDate.Value.Date);
                query = query.Where(r => r.ReportDate >= from);
            }

            if (filter.ToDate.HasValue)
            {
                var toExclusive = new DateTimeOffset(filter.ToDate.Value.Date.AddDays(1));
                query = query.Where(r => r.ReportDate < toExclusive);
            }

            return query;
        }
    }
}
