using Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;
using Microsoft.AspNetCore.Http;

namespace Contracting.Infrustructure.Inteface.business;

/// <summary>
/// Backoffice management of client-facing content (tender documents, schedules, monthly reports).
/// Update methods return null when the target entity does not exist (or is deleted).
/// </summary>
public interface IClientContentService
{
    Task<UpdatedTenderDocumentDto?> UpdateTenderDocumentAsync(
        Guid tenderId, string? title, IFormFile? file, CancellationToken cancellationToken = default);

    Task<UpdatedScheduleDto?> UpdateScheduleAsync(
        Guid scheduleId, string? title, string? version, IFormFile? file, CancellationToken cancellationToken = default);

    Task<UpdatedMonthlyReportDto?> UpdateMonthlyReportAsync(
        Guid reportId, int? month, int? year, string? title, string? workProgress,
        ICollection<IFormFile>? newAttachments, List<Guid>? removeAttachmentIds, CancellationToken cancellationToken = default);
}
