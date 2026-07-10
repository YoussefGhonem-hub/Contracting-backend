using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.client;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.BusinessDtos.VariationOrderDtos;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using Contracting.Shared.Resources;
using Contracting.Shared.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.business;

public class TechnicalVariationOrderService : ITechnicalVariationOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _fileStorage;
    private readonly IStorageService _storageService;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public TechnicalVariationOrderService(
        ApplicationDbContext db,
        IFileStorage fileStorage,
        IStorageService storageService,
        INotificationService notificationService,
        IStringLocalizer<SharedResources> localizer)
    {
        _db = db;
        _fileStorage = fileStorage;
        _storageService = storageService;
        _notificationService = notificationService;
        _localizer = localizer;
    }

    public async Task<GetClientVariationOrderDetailDto?> CreateVariationOrderAsync(CreateVariationOrderDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.ProjectId == Guid.Empty)
            return null;

        var userId = CurrentUser.Id ?? Guid.Empty;
        if (userId == Guid.Empty)
            return null;

        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == userId, cancellationToken);

        if (engineer is null)
            return null;

        var projectExists = await _db.Projects
            .AnyAsync(p => p.Id == dto.ProjectId, cancellationToken);

        if (!projectExists)
            return null;

        var nextNumber = (await _db.VariationOrders
            .Where(v => v.ProjectId == dto.ProjectId)
            .MaxAsync(v => (int?)v.VONumber, cancellationToken) ?? 0) + 1;

        var entity = new VariationOrder
        {
            ProjectId = dto.ProjectId,
            VONumber = nextNumber,
            Title = dto.Title,
            Description = dto.Description,
            Cost = dto.Cost,
            IssueDate = dto.IssueDate ?? Contracting.Shared.Common.DateTimeHelper.DateTimeNow,
            DueDate = dto.DueDate,
            Status = VOStatus.Pending,
            CreatedByEngineerId = engineer.Id,
            Attachments = new List<VariationOrderAttachment>()
        };

        if (dto.Attachments is not null)
        {
            foreach (var file in dto.Attachments.Where(f => f is not null && f.Length > 0))
            {
                var relativePath = await _fileStorage.SaveAsync(file, "uploads/variation-orders", cancellationToken);
                entity.Attachments.Add(new VariationOrderAttachment
                {
                    Key = relativePath,
                    FileName = file.FileName,
                    Extension = Path.GetExtension(file.FileName),
                    FileSize = file.Length,
                    Url = _storageService.GetUploadedFileUrl(relativePath)
                });
            }
        }

        await _db.VariationOrders.AddAsync(entity, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        // Notify all clients linked to this project
        var clientUserIds = await _db.ClientProjects
            .Where(cp => cp.ProjectId == dto.ProjectId && !cp.IsDeleted)
            .Select(cp => cp.Client.ApplicationUserId)
            .ToListAsync(cancellationToken);

        var title = _localizer[SharedResourcesKeys.ClientNotificationVariationCreatedTitle].Value;
        var body = _localizer[SharedResourcesKeys.ClientNotificationVariationCreatedBody].Value;

        foreach (var clientUserId in clientUserIds)
            await _notificationService.SendFanOutNotificationAsync(clientUserId, title, body, entity.Id, null, null, "variation_order");

        return new GetClientVariationOrderDetailDto
        {
            Id = entity.Id,
            VONumber = entity.VONumber,
            Title = entity.Title,
            Description = entity.Description,
            Cost = entity.Cost,
            Status = entity.Status.ToString(),
            IssueDate = entity.IssueDate,
            DueDate = entity.DueDate,
            ClientActionDate = entity.ClientActionDate,
            ClientRejectionReason = entity.ClientRejectionReason,
            Attachments = entity.Attachments.Select(a => new VOAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                Extension = a.Extension,
                FileSize = a.FileSize,
                Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList()
        };
    }
}
