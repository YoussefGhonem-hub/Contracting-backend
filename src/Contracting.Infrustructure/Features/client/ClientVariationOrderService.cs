using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Infrustructure.Features.client;

public class ClientVariationOrderService : IClientVariationOrderService
{
    private readonly ApplicationDbContext _db;
    private readonly IStorageService _storage;
    private readonly INotificationService _notificationService;

    public ClientVariationOrderService(ApplicationDbContext db, IStorageService storage, INotificationService notificationService)
    {
        _db = db;
        _storage = storage;
        _notificationService = notificationService;
    }

    public async Task<GetClientVariationOrdersDto?> GetVariationOrdersAsync(
        Guid projectId, string? status, CancellationToken cancellationToken = default)
    {
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
            return null;

        var query = _db.VariationOrders.Where(v => v.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<VOStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(v => v.Status == parsedStatus);

        var items = await query
            .OrderByDescending(v => v.VONumber)
            .Select(v => new GetClientVariationOrderListItemDto
            {
                Id = v.Id,
                VONumber = v.VONumber,
                Title = v.Title,
                Cost = v.Cost,
                Status = v.Status.ToString(),
                IssueDate = v.IssueDate,
                DueDate = v.DueDate
            })
            .ToListAsync(cancellationToken);

        // Aggregates always across ALL VOs for the project (ignore status filter)
        var allCosts = await _db.VariationOrders
            .Where(v => v.ProjectId == projectId)
            .Select(v => new { v.Status, v.Cost })
            .ToListAsync(cancellationToken);

        return new GetClientVariationOrdersDto
        {
            TotalApproved = allCosts.Where(v => v.Status == VOStatus.Approved).Sum(v => v.Cost),
            TotalPending = allCosts.Where(v => v.Status == VOStatus.Pending).Sum(v => v.Cost),
            VariationOrders = items
        };
    }

    public async Task<GetClientVariationOrderDetailDto?> GetVariationOrderByIdAsync(
        Guid voId, CancellationToken cancellationToken = default)
    {
        var vo = await _db.VariationOrders
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == voId, cancellationToken);

        if (vo is null) return null;

        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, vo.ProjectId, cancellationToken))
            return null;

        return MapToDetail(vo);
    }

    public async Task<GetClientVariationOrderDetailDto?> ApproveVariationOrderAsync(
        Guid voId, CancellationToken cancellationToken = default)
    {
        var vo = await _db.VariationOrders
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == voId, cancellationToken);

        if (vo is null || vo.Status != VOStatus.Pending) return null;

        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, vo.ProjectId, cancellationToken))
            return null;

        vo.Status = VOStatus.Approved;
        vo.ClientActionDate = Contracting.Shared.Common.DateTimeHelper.DateTimeNow;
        vo.ClientRejectionReason = null;

        await _db.SaveChangesAsync(cancellationToken);

        // Notify the engineer who created the variation order
        if (vo.CreatedByEngineerId != Guid.Empty)
        {
            var engineerUserId = await _db.Engineers
                .Where(e => e.Id == vo.CreatedByEngineerId)
                .Select(e => e.ApplicationUserId)
                .FirstOrDefaultAsync(cancellationToken);
            if (engineerUserId != Guid.Empty)
                await _notificationService.SendFanOutNotificationAsync(engineerUserId,
                    "Variation Order Approved",
                    $"Variation Order #{vo.VONumber} \"{vo.Title}\" has been approved by the client.",
                    vo.Id, null, null, "variation_order");
        }

        return MapToDetail(vo);
    }

    public async Task<GetClientVariationOrderDetailDto?> RejectVariationOrderAsync(
        Guid voId, string? rejectionReason, CancellationToken cancellationToken = default)
    {
        var vo = await _db.VariationOrders
            .Include(v => v.Attachments)
            .FirstOrDefaultAsync(v => v.Id == voId, cancellationToken);

        if (vo is null || vo.Status != VOStatus.Pending) return null;

        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, vo.ProjectId, cancellationToken))
            return null;

        vo.Status = VOStatus.Rejected;
        vo.ClientActionDate = Contracting.Shared.Common.DateTimeHelper.DateTimeNow;
        vo.ClientRejectionReason = rejectionReason;

        await _db.SaveChangesAsync(cancellationToken);

        // Notify the engineer who created the variation order
        if (vo.CreatedByEngineerId != Guid.Empty)
        {
            var engineerUserId = await _db.Engineers
                .Where(e => e.Id == vo.CreatedByEngineerId)
                .Select(e => e.ApplicationUserId)
                .FirstOrDefaultAsync(cancellationToken);
            if (engineerUserId != Guid.Empty)
                await _notificationService.SendFanOutNotificationAsync(engineerUserId,
                    "Variation Order Rejected",
                    $"Variation Order #{vo.VONumber} \"{vo.Title}\" has been rejected by the client.",
                    vo.Id, null, null, "variation_order");
        }

        return MapToDetail(vo);
    }

    private GetClientVariationOrderDetailDto MapToDetail(Domain.Entities.client.VariationOrder vo)
        => new()
        {
            Id = vo.Id,
            VONumber = vo.VONumber,
            Title = vo.Title,
            Description = vo.Description,
            Cost = vo.Cost,
            Status = vo.Status.ToString(),
            IssueDate = vo.IssueDate,
            DueDate = vo.DueDate,
            ClientActionDate = vo.ClientActionDate,
            ClientRejectionReason = vo.ClientRejectionReason,
            // Stored URLs target a private bucket (Access Denied); return pre-signed URLs.
            Attachments = vo.Attachments.Select(a => new VOAttachmentDto
            {
                Id = a.Id,
                FileName = a.FileName,
                Extension = a.Extension,
                FileSize = a.FileSize,
                Url = _storage.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList()
        };
}
