using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using Contracting.Shared.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.business;

public class ClientContentService : IClientContentService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly IStorageService _storageService;

    public ClientContentService(ApplicationDbContext db, IFileStorage fileStorage, IStorageService storageService)
    {
        _db = db;
        _fileStorage = fileStorage;
        _storageService = storageService;
    }

    public async Task<UpdatedTenderDocumentDto?> UpdateTenderDocumentAsync(
        Guid tenderId, string? title, IFormFile? file, CancellationToken cancellationToken = default)
    {
        var tender = await _db.TenderDocuments
            .FirstOrDefaultAsync(t => t.Id == tenderId && !t.IsDeleted, cancellationToken);
        if (tender is null) return null;

        var userId = CurrentUser.Id ?? Guid.Empty;

        if (title is not null)
            tender.Title = title;

        if (file is { Length: > 0 })
        {
            var oldKey = tender.Key;
            var relativePath = await _fileStorage.SaveAsync(file, "uploads/tender", cancellationToken);
            tender.Key = relativePath;
            tender.FileName = file.FileName;
            tender.Extension = Path.GetExtension(file.FileName);
            tender.FileSize = file.Length;
            tender.Url = _storageService.GetUploadedFileUrl(relativePath);

            if (!string.IsNullOrWhiteSpace(oldKey))
                await _fileStorage.DeleteAsync(oldKey, cancellationToken);
        }

        tender.MarkAsModified(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return new UpdatedTenderDocumentDto
        {
            Id = tender.Id,
            ProjectId = tender.ProjectId,
            Title = tender.Title,
            Url = tender.Url,
            FileName = tender.FileName
        };
    }

    public async Task<UpdatedScheduleDto?> UpdateScheduleAsync(
        Guid scheduleId, string? title, string? version, IFormFile? file, CancellationToken cancellationToken = default)
    {
        var schedule = await _db.ProjectSchedules
            .FirstOrDefaultAsync(s => s.Id == scheduleId && !s.IsDeleted, cancellationToken);
        if (schedule is null) return null;

        var userId = CurrentUser.Id ?? Guid.Empty;

        if (title is not null)
            schedule.Title = title;
        if (version is not null)
            schedule.Version = version;

        if (file is { Length: > 0 })
        {
            var oldKey = schedule.Key;
            var relativePath = await _fileStorage.SaveAsync(file, "uploads/schedules", cancellationToken);
            schedule.Key = relativePath;
            schedule.FileName = file.FileName;
            schedule.Extension = Path.GetExtension(file.FileName);
            schedule.FileSize = file.Length;
            schedule.Url = _storageService.GetUploadedFileUrl(relativePath);

            if (!string.IsNullOrWhiteSpace(oldKey))
                await _fileStorage.DeleteAsync(oldKey, cancellationToken);
        }

        schedule.MarkAsModified(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return new UpdatedScheduleDto
        {
            Id = schedule.Id,
            ProjectId = schedule.ProjectId,
            Title = schedule.Title,
            Version = schedule.Version,
            Url = schedule.Url,
            FileName = schedule.FileName
        };
    }

    public async Task<UpdatedMonthlyReportDto?> UpdateMonthlyReportAsync(
        Guid reportId, int? month, int? year, string? title, string? workProgress,
        ICollection<IFormFile>? newAttachments, List<Guid>? removeAttachmentIds, CancellationToken cancellationToken = default)
    {
        var report = await _db.ClientMonthlyReports
            .Include(r => r.Attachments)
            .FirstOrDefaultAsync(r => r.Id == reportId && !r.IsDeleted, cancellationToken);
        if (report is null) return null;

        var userId = CurrentUser.Id ?? Guid.Empty;

        if (month is > 0)
            report.Month = month.Value;
        if (year is > 0)
            report.Year = year.Value;
        if (title is not null)
            report.Title = title;
        if (workProgress is not null)
            report.WorkProgress = workProgress;

        // Remove selected attachments (and their stored files)
        if (removeAttachmentIds is { Count: > 0 })
        {
            var toRemove = report.Attachments
                .Where(a => removeAttachmentIds.Contains(a.Id))
                .ToList();

            foreach (var att in toRemove)
            {
                if (!string.IsNullOrWhiteSpace(att.Key))
                    await _fileStorage.DeleteAsync(att.Key, cancellationToken);

                report.Attachments.Remove(att);
                _db.ClientMonthlyReportAttachments.Remove(att);
            }
        }

        // Append new attachments
        if (newAttachments is not null)
        {
            foreach (var file in newAttachments.Where(f => f is not null && f.Length > 0))
            {
                var relativePath = await _fileStorage.SaveAsync(file, "uploads/monthly-reports", cancellationToken);
                report.Attachments.Add(new ClientMonthlyReportAttachment
                {
                    Key = relativePath,
                    FileName = file.FileName,
                    Extension = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                    Url = _storageService.GetUploadedFileUrl(relativePath)
                });
            }
        }

        report.MarkAsModified(userId);
        await _db.SaveChangesAsync(cancellationToken);

        return new UpdatedMonthlyReportDto
        {
            Id = report.Id,
            ProjectId = report.ProjectId,
            Month = report.Month,
            Year = report.Year,
            Title = report.Title,
            WorkProgress = report.WorkProgress,
            Attachments = report.Attachments.Select(a => new MonthlyReportAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url,
                FileSize = a.FileSize
            }).ToList()
        };
    }
}
