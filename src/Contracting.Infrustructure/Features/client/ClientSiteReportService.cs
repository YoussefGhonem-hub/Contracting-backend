using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ReportDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientSiteReportService : IClientSiteReportService
{
    private readonly ApplicationDbContext _db;

    public ClientSiteReportService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<List<GetClientSiteReportListItemDto>?> GetClientSiteReportsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        // Verify the client owns this project
        var isClientProject = await _db.ClientProjects
            .AnyAsync(cp => cp.ProjectId == projectId && cp.Client != null && cp.Client.ApplicationUserId == userId, cancellationToken);

        if (!isClientProject)
            return null;

        var reports = await _db.ClientMonthlyReports
            .Where(r => r.ProjectId == projectId)
            .OrderByDescending(r => r.Year).ThenByDescending(r => r.Month)
            .Select(r => new GetClientSiteReportListItemDto
            {
                Id = r.Id,
                Title = r.Title,
                Month = r.Month,
                Year = r.Year,
                CreatedDate = r.CreatedDate
            })
            .ToListAsync(cancellationToken);

        return reports;
    }

    public async Task<GetClientSiteReportDetailDto?> GetClientSiteReportByIdAsync(Guid reportId, CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var report = await _db.ClientMonthlyReports
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report is null)
            return null;

        // Verify the client owns the project this report belongs to
        var isClientProject = await _db.ClientProjects
            .AnyAsync(cp => cp.ProjectId == report.ProjectId && cp.Client != null && cp.Client.ApplicationUserId == userId, cancellationToken);

        if (!isClientProject)
            return null;

        return new GetClientSiteReportDetailDto
        {
            Id = report.Id,
            ProjectId = report.ProjectId,
            Title = report.Title,
            WorkProgress = report.WorkProgress,
            Month = report.Month,
            Year = report.Year,
            CreatedDate = report.CreatedDate,
            Attachments = report.Attachments.Select(a => new ClientSiteReportAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                Extension = a.Extension,
                FileSize = a.FileSize,
                Url = a.Url
            }).ToList()
        };
    }
}
