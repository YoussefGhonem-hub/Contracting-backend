using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ScheduleDtos;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ClientScheduleService : IClientScheduleService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ClientScheduleService(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<GetClientScheduleDto?> GetScheduleAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
            return null;

        var clientProject = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => new
            {
                p.StartDate,
                p.ExpectedEndDate,
                p.ProgressPercent
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (clientProject is null)
            return null;

        // Compute duration in weeks
        int? durationWeeks = null;
        if (clientProject.StartDate.HasValue && clientProject.ExpectedEndDate.HasValue)
        {
            var days = (clientProject.ExpectedEndDate.Value - clientProject.StartDate.Value).TotalDays;
            durationWeeks = (int)Math.Round(days / 7.0);
        }

        // Tab 1: flat list of all monthly report attachments, newest first
        var monthlyReportRows = await _db.ClientMonthlyReports
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .SelectMany(r => r.Attachments.Select(a => new
            {
                a.Id,
                r.Month,
                r.Year,
                a.FileName,
                a.Extension,
                a.FileSize,
                a.Key,
                a.Url
            }))
            .ToListAsync(cancellationToken);

        // Stored URLs target a private bucket (Access Denied); return pre-signed URLs.
        var monthlyReports = monthlyReportRows.Select(a => new ScheduleMonthlyReportFileDto
        {
            Id = a.Id,
            Month = a.Month,
            Year = a.Year,
            FileName = a.FileName,
            Extension = a.Extension,
            FileSize = a.FileSize,
            Url = _storage.GetPreSignedUrl(a.Key) ?? a.Url
        }).ToList();

        // Tab 2: project timeline / schedule documents
        var timelineRows = await _db.ProjectSchedules
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedDate)
            .Select(s => new
            {
                s.Id,
                s.Title,
                s.Version,
                s.FileName,
                s.Extension,
                s.FileSize,
                s.Key,
                s.Url
            })
            .ToListAsync(cancellationToken);

        var timelineDocs = timelineRows.Select(s => new ScheduleTimelineDocumentDto
        {
            Id = s.Id,
            Title = s.Title,
            Version = s.Version,
            FileName = s.FileName,
            Extension = s.Extension,
            FileSize = s.FileSize,
            Url = _storage.GetPreSignedUrl(s.Key) ?? s.Url
        }).ToList();

        return new GetClientScheduleDto
        {
            StartDate = clientProject.StartDate,
            CompletionDate = clientProject.ExpectedEndDate,
            DurationWeeks = durationWeeks,
            ProgressPercent = clientProject.ProgressPercent,
            MonthlyReports = monthlyReports,
            TimelineDocuments = timelineDocs
        };
    }
}
