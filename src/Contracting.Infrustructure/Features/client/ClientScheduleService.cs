using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ScheduleDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientScheduleService : IClientScheduleService
{
    private readonly ApplicationDbContext _db;

    public ClientScheduleService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<GetClientScheduleDto?> GetScheduleAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var clientProject = await _db.ClientProjects
            .Where(cp => cp.ProjectId == projectId
                      && cp.Client != null
                      && cp.Client.ApplicationUserId == userId)
            .Select(cp => new
            {
                cp.Project.StartDate,
                cp.Project.ExpectedEndDate,
                cp.Project.ProgressPercent
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
        var monthlyReports = await _db.ClientMonthlyReports
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .SelectMany(r => r.Attachments.Select(a => new ScheduleMonthlyReportFileDto
            {
                Id = a.Id,
                Month = r.Month,
                Year = r.Year,
                FileName = a.FileName,
                Extension = a.Extension,
                FileSize = a.FileSize,
                Url = a.Url
            }))
            .ToListAsync(cancellationToken);

        // Tab 2: project timeline / schedule documents
        var timelineDocs = await _db.ProjectSchedules
            .Where(s => s.ProjectId == projectId)
            .OrderByDescending(s => s.CreatedDate)
            .Select(s => new ScheduleTimelineDocumentDto
            {
                Id = s.Id,
                Title = s.Title,
                Version = s.Version,
                FileName = s.FileName,
                Extension = s.Extension,
                FileSize = s.FileSize,
                Url = s.Url
            })
            .ToListAsync(cancellationToken);

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
