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
                EngineerId = engineer.Id,
                ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId,
                ReportDate = dto.ReportDate ?? DateTimeOffset.UtcNow,
                GeneralNotes = dto.GeneralNotes,
                SiteSafetyObservations = dto.SiteSafetyObservations,
                QualityControlObservations = dto.QualityControlObservations,
                WorkLogs = new List<EngineerSiteWorkLog>(),
                Materials = new List<EngineerSiteMaterial>(),
                Equipments = new List<EngineerSiteEquipment>(),
                SurveyQuestions = new List<EngineerSiteSurveyQuestion>()
            };

            if (dto.WorkLogs != null && dto.WorkLogs.Any())
            {
                foreach (var workLogDto in dto.WorkLogs)
                {
                    var workLog = new EngineerSiteWorkLog
                    {
                        name = workLogDto.name,
                        description = workLogDto.description,
                        quantity = workLogDto.quantity,
                        totalHours = workLogDto.totalHours,
                        totalHoursToDate = workLogDto.totalHoursToDate,
                        Attachments = new List<EngineerSiteWorkLogAttachment>()
                    };

                    if (workLogDto.Attachments != null && workLogDto.Attachments.Any())
                    {
                        var uploaded = await _storageService.UploadFiles(workLogDto.Attachments.ToList());
                        workLog.Attachments = uploaded?.Select(f => new EngineerSiteWorkLogAttachment
                        {
                            Key = f.Key,
                            FileName = f.FileName,
                            Extension = f.Extension,
                            FileSize = f.FileSize,
                            Url = f.Url
                        }).ToList() ?? new List<EngineerSiteWorkLogAttachment>();
                    }

                    report.WorkLogs.Add(workLog);
                }
            }

            if (dto.Materials != null && dto.Materials.Any())
            {
                report.Materials = dto.Materials.Select(m => new EngineerSiteMaterial
                {
                    name = m.name,
                    quantity = m.quantity,
                    usage = m.usage,
                    needMore = m.needMore,
                    unit = m.unit,
                    unitCost = m.unitCost,
                    totalCost = m.totalCost,
                    notes = m.notes
                }).ToList();
            }

            if (dto.Equipments != null && dto.Equipments.Any())
            {
                report.Equipments = dto.Equipments.Select(e => new EngineerSiteEquipment
                {
                    name = e.name,
                    quantity = e.quantity,
                    hoursUsed = e.hoursUsed,
                    condition = e.condition,
                    isOperational = e.isOperational,
                    notes = e.notes
                }).ToList();
            }

            if (dto.SurveyQuestions != null && dto.SurveyQuestions.Any())
            {
                report.SurveyQuestions = dto.SurveyQuestions.Select(s => new EngineerSiteSurveyQuestion
                {
                    TemplateId = s.TemplateId,
                    question = s.question,
                    answer = s.answer,
                    description = s.description
                }).ToList();
            }
            else
            {
                var templates = await _db.EngineerSiteSurveyQuestionTemplates
                    .Where(t => t.isActive)
                    .OrderBy(t => t.order)
                    .AsNoTracking()
                    .ToListAsync();

                if (templates.Any())
                {
                    report.SurveyQuestions = templates.Select(t => new EngineerSiteSurveyQuestion
                    {
                        TemplateId = t.Id,
                        question = t.question,
                        answer = null,
                        description = null
                    }).ToList();
                }
            }

            await _db.EngineerSiteReports.AddAsync(report);
            await _db.SaveChangesAsync();

            var createdReport = await _db.EngineerSiteReports
                .Include(r => r.Project)
                .Include(r => r.Engineer)
                .Include(r => r.WorkLogs)
                    .ThenInclude(w => w.Attachments)
                .Include(r => r.Materials)
                .Include(r => r.Equipments)
                .Include(r => r.SurveyQuestions)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == report.Id);

            return createdReport is null ? null! : _mapper.Map<GetEngineerSiteReportDto>(createdReport);
        }

        public async Task<GetEngineerSiteReportDto> GetEngineerSiteReportByIdAsync(Guid reportId)
        {
            var report = await _db.EngineerSiteReports
                .Include(r => r.Project)
                .Include(r => r.Engineer)
                .Include(r => r.WorkLogs)
                    .ThenInclude(w => w.Attachments)
                .Include(r => r.Materials)
                .Include(r => r.Equipments)
                .Include(r => r.SurveyQuestions)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == reportId);

            return report is null ? null! : _mapper.Map<GetEngineerSiteReportDto>(report);
        }

        public async Task<List<GetEngineerSiteReportDto>> GetMyEngineerSiteReportsAsync(EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default)
        {
            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId), cancellationToken);

            if (engineer is null)
            {
                return new List<GetEngineerSiteReportDto>();
            }

            var query = _db.EngineerSiteReports
                .Where(r => r.EngineerId == engineer.Id)
                .Include(r => r.Project)
                .Include(r => r.Engineer)
                .Include(r => r.WorkLogs)
                    .ThenInclude(w => w.Attachments)
                .Include(r => r.Materials)
                .Include(r => r.Equipments)
                .Include(r => r.SurveyQuestions)
                .AsNoTracking();

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

            var reports = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<GetEngineerSiteReportDto>>(reports);
        }

        public async Task<PaginatedList<GetEngineerSiteReportDto>> GetEngineerSiteReportsByEngineerIdAsync(Guid engineerId, EngineerSiteReportFilterDto filter, CancellationToken cancellationToken = default)
        {
            var query = _db.EngineerSiteReports
                .Where(r => r.EngineerId == engineerId)
                .Include(r => r.Project)
                .Include(r => r.Engineer)
                .Include(r => r.WorkLogs)
                    .ThenInclude(w => w.Attachments)
                .Include(r => r.Materials)
                .Include(r => r.Equipments)
                .Include(r => r.SurveyQuestions)
                .AsNoTracking();

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
    }
}
