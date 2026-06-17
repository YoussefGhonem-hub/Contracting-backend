using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.ReportDtos;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ClientSiteReportService : IClientSiteReportService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;

    public ClientSiteReportService(ApplicationDbContext db, IStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<List<GetClientSiteReportListItemDto>?> GetClientSiteReportsAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        // Clients are limited to their own projects; staff may access any project.
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
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
        var report = await _db.ClientMonthlyReports
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == reportId, cancellationToken);

        if (report is null)
            return null;

        // Clients are limited to their own projects; staff may access any project.
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, report.ProjectId, cancellationToken))
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
            // Stored URLs target a private bucket (Access Denied); return pre-signed URLs.
            Attachments = report.Attachments.Select(a => new ClientSiteReportAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                Extension = a.Extension,
                FileSize = a.FileSize,
                Url = _storage.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList()
        };
    }
}
