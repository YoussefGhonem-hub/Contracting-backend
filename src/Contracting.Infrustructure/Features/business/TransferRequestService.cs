using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using Contracting.Shared.Resources;
using ErrorOr;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace Contracting.Infrustructure.Features.business
{
    public class TransferRequestService : ITransferRequestService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;
        private readonly INotificationService _notificationService;
        private readonly IStringLocalizer<SharedResources> _localizer;
        private readonly ILogger<TransferRequestService> _logger;

        public TransferRequestService(
            ApplicationDbContext db,
            Storage.AWS3.Services.IStorageService storageService,
            INotificationService notificationService,
            IStringLocalizer<SharedResources> localizer,
            ILogger<TransferRequestService> logger)
        {
            _db = db;
            _storageService = storageService;
            _notificationService = notificationService;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task<ErrorOr<GetTransferRequestDto>> CreateAsync(CreateTransferRequestDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            if (dto.DestinationProjectId == null && string.IsNullOrWhiteSpace(dto.DestinationWarehouse))
                return Error.Validation("TransferRequest.DestinationRequired", "Destination project or warehouse must be selected.");

            if (dto.Items == null || dto.Items.Count == 0)
                return Error.Validation("TransferRequest.ItemsRequired", "At least one item must be added.");

            var request = new TransferRequest
            {
                RequestNumber = await GenerateRequestNumberAsync(),
                RequestDate = dto.RequestDate,
                SourceProjectId = dto.SourceProjectId == Guid.Empty ? null : dto.SourceProjectId,
                SourceWarehouse = dto.SourceWarehouse,
                DestinationProjectId = dto.DestinationProjectId == Guid.Empty ? null : dto.DestinationProjectId,
                DestinationWarehouse = dto.DestinationWarehouse,
                RequestedById = engineer?.Id,
                Notes = dto.Notes,
                StatusId = s.New
            };

            foreach (var itemDto in dto.Items)
            {
                if (itemDto.Quantity <= 0)
                    return Error.Validation("TransferRequest.InvalidQuantity", "Item quantity must be greater than zero.");

                request.Items.Add(new TransferRequestItem
                {
                    ItemCode = itemDto.ItemCode,
                    ItemName = itemDto.ItemName,
                    Unit = itemDto.Unit,
                    Quantity = itemDto.Quantity,
                    Notes = itemDto.Notes
                });
            }

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                    foreach (var f in uploaded)
                        request.Attachments.Add(new TransferRequestAttachment
                        {
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            request.Activities.Add(new TransferRequestActivity
            {
                EngineerId = engineer?.Id,
                ToStatusId = s.New,
                ActionType = "Submitted"
            });

            await _db.TransferRequests.AddAsync(request);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GetTransferRequestDto>> UpdateAsync(UpdateTransferRequestDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var request = await _db.TransferRequests
                .Include(r => r.Items)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");
            if (request.StatusId != s.New)
                return Error.Validation("TransferRequest.CannotEdit", "Only PendingReceipt requests can be edited.");

            if (dto.RequestDate.HasValue) request.RequestDate = dto.RequestDate.Value;
            if (dto.SourceProjectId.HasValue) request.SourceProjectId = dto.SourceProjectId == Guid.Empty ? null : dto.SourceProjectId;
            if (dto.SourceWarehouse is not null) request.SourceWarehouse = dto.SourceWarehouse;
            if (dto.DestinationProjectId.HasValue) request.DestinationProjectId = dto.DestinationProjectId == Guid.Empty ? null : dto.DestinationProjectId;
            if (dto.DestinationWarehouse is not null) request.DestinationWarehouse = dto.DestinationWarehouse;
            if (dto.Notes is not null) request.Notes = dto.Notes;

            if (dto.Items != null)
            {
                _db.TransferRequestItems.RemoveRange(request.Items);
                foreach (var itemDto in dto.Items)
                {
                    if (itemDto.Quantity <= 0)
                        return Error.Validation("TransferRequest.InvalidQuantity", "Item quantity must be greater than zero.");
                    request.Items.Add(new TransferRequestItem
                    {
                        TransferRequestId = request.Id,
                        ItemCode = itemDto.ItemCode, ItemName = itemDto.ItemName,
                        Unit = itemDto.Unit, Quantity = itemDto.Quantity, Notes = itemDto.Notes
                    });
                }
            }

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                    foreach (var f in uploaded)
                        request.Attachments.Add(new TransferRequestAttachment
                        {
                            TransferRequestId = request.Id,
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var request = await _db.TransferRequests.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");
            if (request.StatusId != s.New)
                return Error.Validation("TransferRequest.CannotDelete", "Only PendingReceipt requests can be deleted.");

            request.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult("Deleted successfully.");
        }

        public async Task<ErrorOr<GetTransferRequestDto>> GetByIdAsync(Guid id)
        {
            var request = await _db.TransferRequests
                .Include(r => r.SourceProject)
                .Include(r => r.DestinationProject)
                .Include(r => r.RequestedBy)
                .Include(r => r.Status)
                .Include(r => r.Items)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
                .Include(r => r.Activities).ThenInclude(a => a.FromStatus)
                .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");
            return MapToDto(request);
        }

        public async Task<PaginatedList<GetTransferRequestDto>> GetAllAsync(TransferRequestFilterDto filter)
        {
            var query = _db.TransferRequests
                .Include(r => r.SourceProject)
                .Include(r => r.DestinationProject)
                .Include(r => r.RequestedBy).ThenInclude(e => e.Department)
                .Include(r => r.Status)
                .Include(r => r.Items)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
                .Include(r => r.Activities).ThenInclude(a => a.FromStatus)
                .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
                .Where(r => !r.IsDeleted)
                .AsNoTracking();

            // Branch isolation: Admins see all; everyone else sees only their branch.
            var roles = CurrentUser.Roles;
            var isAdmin = roles.Any(r =>
                r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

            if (!isAdmin)
            {
                var engineer = await _db.Engineers
                    .Include(e => e.Department)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

                var userBranchId = engineer?.Department?.BranchId;

                if (userBranchId.HasValue)
                {
                    query = query.Where(r =>
                        (r.SourceProject != null && r.SourceProject.BranchId == userBranchId) ||
                        (r.DestinationProject != null && r.DestinationProject.BranchId == userBranchId) ||
                        (r.SourceProject == null && r.DestinationProject == null &&
                         r.RequestedBy != null && r.RequestedBy.Department != null &&
                         r.RequestedBy.Department.BranchId == userBranchId));
                }
                else
                {
                    // Fail closed: a non-admin user whose branch cannot be resolved
                    // (no engineer record or no department/branch) must never see other
                    // branches' data — restrict to their own requests only.
                    var engineerId = engineer?.Id ?? Guid.Empty;
                    query = query.Where(r => r.RequestedById == engineerId);
                }
            }

            if (filter.StatusId.HasValue)
                query = query.Where(r => r.StatusId == filter.StatusId);

            if (filter.SourceProjectId.HasValue)
                query = query.Where(r => r.SourceProjectId == filter.SourceProjectId);

            if (filter.DestinationProjectId.HasValue)
                query = query.Where(r => r.DestinationProjectId == filter.DestinationProjectId);

            if (filter.RequestedById.HasValue)
                query = query.Where(r => r.RequestedById == filter.RequestedById);

            if (filter.FromDate.HasValue) query = query.Where(r => r.RequestDate >= filter.FromDate);
            if (filter.ToDate.HasValue) query = query.Where(r => r.RequestDate <= filter.ToDate);

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(r => r.RequestNumber!.Contains(filter.Search));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedDate)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedList<GetTransferRequestDto>(
                items.Select(MapToDto).ToList(),
                totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<ErrorOr<GetTransferRequestDto>> TakeActionAsync(Guid id, TransferRequestActionDto dto)
        {
            var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);
            var request = await _db.TransferRequests
                .Include(r => r.RequestedBy).ThenInclude(e => e!.Department)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            var fromStatusId = request.StatusId;
            Guid toStatusId;
            var actionLower = dto.ActionType.Trim().ToLower();

            switch (actionLower)
            {
                case "submit":
                    if (request.StatusId != s.InProgress)
                        return Error.Validation("TransferRequest.InvalidAction", "Only Draft requests can be submitted.");
                    toStatusId = s.New;
                    break;
                case "confirmreceipt":
                    if (request.StatusId != s.New)
                        return Error.Validation("TransferRequest.InvalidAction", "Request must be in PendingReceipt status.");
                    if (dto.Items.Any(i => i.ReceivedQuantity < 0))
                        return Error.Validation("TransferRequest.InvalidQuantity", "Received quantity cannot be negative.");
                    toStatusId = s.Completed;
                    break;
                case "confirmpartialreceipt":
                    if (request.StatusId != s.New)
                        return Error.Validation("TransferRequest.InvalidAction", "Request must be in PendingReceipt status.");
                    if (!dto.Items.Any())
                        return Error.Validation("TransferRequest.ItemsRequired", "At least one item with received quantity is required for partial receipt.");
                    if (dto.Items.Any(i => i.ReceivedQuantity < 0))
                        return Error.Validation("TransferRequest.InvalidQuantity", "Received quantity cannot be negative.");
                    toStatusId = s.Completed;
                    break;
                case "acknowledge":
                    if (!request.NeedsAcknowledgment)
                        return Error.Validation("TransferRequest.NoAcknowledgmentNeeded", "This request does not require acknowledgment.");
                    toStatusId = request.StatusId ?? s.Completed;
                    break;
                case "cancel":
                    if (request.StatusId == s.Completed)
                        return Error.Validation("TransferRequest.InvalidAction", "Closed requests cannot be cancelled.");
                    toStatusId = s.Rejected;
                    break;
                default:
                    return Error.Validation("TransferRequest.UnknownAction", $"Unknown action: {dto.ActionType}");
            }

            // Save received quantities per item for confirm actions
            if ((actionLower == "confirmreceipt" || actionLower == "confirmpartialreceipt") && dto.Items.Any())
            {
                var itemIds = dto.Items.Select(i => i.ItemId).ToList();
                var items = await _db.TransferRequestItems
                    .Where(i => i.TransferRequestId == id && itemIds.Contains(i.Id))
                    .ToListAsync();

                foreach (var item in items)
                {
                    var receipt = dto.Items.FirstOrDefault(i => i.ItemId == item.Id);
                    if (receipt is not null)
                        item.ReceivedQuantity = Math.Min(receipt.ReceivedQuantity, item.Quantity);
                }

                // For ConfirmReceipt: any item not explicitly listed defaults to full quantity
                if (actionLower == "confirmreceipt")
                {
                    var specifiedIds = itemIds.ToHashSet();
                    var unspecifiedItems = await _db.TransferRequestItems
                        .Where(i => i.TransferRequestId == id && !specifiedIds.Contains(i.Id) && i.ReceivedQuantity == null)
                        .ToListAsync();
                    foreach (var item in unspecifiedItems)
                        item.ReceivedQuantity = item.Quantity;
                }
            }

            // Determine NeedsAcknowledgment flag changes
            bool needsAcknowledgment = request.NeedsAcknowledgment;
            bool isCompletionAction = actionLower == "confirmreceipt" || actionLower == "confirmpartialreceipt";
            List<Guid> notifyDepartmentIds = new();

            if (isCompletionAction)
            {
                // Resolve the branch — prefer requester's department, fall back to source/destination project
                Guid? branchId = request.RequestedBy?.Department?.BranchId;

                if (!branchId.HasValue && request.SourceProjectId.HasValue)
                    branchId = await _db.Projects
                        .Where(p => p.Id == request.SourceProjectId.Value)
                        .Select(p => (Guid?)p.BranchId)
                        .FirstOrDefaultAsync();

                if (!branchId.HasValue && request.DestinationProjectId.HasValue)
                    branchId = await _db.Projects
                        .Where(p => p.Id == request.DestinationProjectId.Value)
                        .Select(p => (Guid?)p.BranchId)
                        .FirstOrDefaultAsync();

                if (branchId.HasValue)
                {
                    // All departments in this branch that opted in to transfer notifications
                    notifyDepartmentIds = await _db.Departmentes
                        .Where(d => d.BranchId == branchId.Value && d.NotifyOnTransferComplete && !d.IsDeleted)
                        .Select(d => d.Id)
                        .ToListAsync();

                    if (notifyDepartmentIds.Any())
                        needsAcknowledgment = true;
                }
            }
            else if (actionLower == "acknowledge")
            {
                needsAcknowledgment = false;
            }

            await _db.TransferRequests
                .Where(r => r.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.StatusId, toStatusId)
                    .SetProperty(r => r.NeedsAcknowledgment, needsAcknowledgment)
                    .SetProperty(r => r.ModifiedDate, DateTimeOffset.UtcNow)
                    .SetProperty(r => r.ModifiedBy, engineer != null ? engineer.Id : (Guid?)null));

            await _db.TransferRequestActivities.AddAsync(new TransferRequestActivity
            {
                TransferRequestId = id,
                EngineerId = engineer?.Id,
                FromStatusId = fromStatusId,
                ToStatusId = toStatusId,
                ActionType = dto.ActionType,
                Comments = dto.Comments
            });

            await _db.SaveChangesAsync();

            // Notify every engineer in every opted-in department (same branch) when the transfer
            // request is completed. Both membership paths (join-table and legacy FK) are covered.
            // This whole block runs AFTER the status/activity SaveChangesAsync above, so a failure
            // here must never look like it silently "did nothing" - every branch is logged so a
            // missing notification can actually be diagnosed instead of leaving zero trace.
            if (isCompletionAction && notifyDepartmentIds.Any())
            {
                var title = _localizer[SharedResourcesKeys.NotificationTransferCompletedTitle].Value;
                var body  = _localizer[SharedResourcesKeys.NotificationTransferCompletedBody].Value;

                foreach (var deptId in notifyDepartmentIds)
                {
                    try
                    {
                        var fromJoinTable = await _db.EngineerDepartments
                            .Where(ed => ed.DepartmentId == deptId && !ed.Engineer!.IsDeleted)
                            .Select(ed => ed.Engineer!.ApplicationUserId)
                            .ToListAsync();

                        var fromLegacy = await _db.Engineers
                            .Where(e => e.DepartmentId == deptId && !e.IsDeleted)
                            .Select(e => e.ApplicationUserId)
                            .ToListAsync();

                        var recipients = fromJoinTable
                            .Union(fromLegacy)
                            .Where(uid => uid != Guid.Empty)
                            .Distinct()
                            .ToList();

                        if (recipients.Count == 0)
                        {
                            _logger.LogWarning(
                                "TransferRequest {RequestId} completion: department {DepartmentId} is opted in to NotifyOnTransferComplete but has no engineers with a linked ApplicationUserId - nobody to notify.",
                                id, deptId);
                            continue;
                        }

                        foreach (var engineerUserId in recipients)
                        {
                            try
                            {
                                await _notificationService.SendFanOutNotificationAsync(
                                    engineerUserId,
                                    title,
                                    body,
                                    id,
                                    deptId,
                                    null,
                                    "Transfer");
                            }
                            catch (Exception ex)
                            {
                                // A failure notifying one recipient must not stop the rest of the
                                // department (or the other opted-in departments) from being notified.
                                _logger.LogError(ex,
                                    "TransferRequest {RequestId} completion: failed to notify user {UserId} in department {DepartmentId}.",
                                    id, engineerUserId, deptId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            "TransferRequest {RequestId} completion: failed to resolve/notify recipients for department {DepartmentId}.",
                            id, deptId);
                    }
                }
            }

            // Notify the original requester directly whenever their request reaches a terminal
            // state - this is independent of the opt-in department fan-out above (which is a
            // separate "let other departments know too" feature) and must fire regardless of
            // whether any department has NotifyOnTransferComplete enabled.
            if ((isCompletionAction || actionLower == "cancel") && request.RequestedBy is not null)
            {
                try
                {
                    var (titleKey, bodyKey) = isCompletionAction
                        ? (SharedResourcesKeys.NotificationTransferCompletedTitle, SharedResourcesKeys.NotificationTransferCompletedBody)
                        : (SharedResourcesKeys.NotificationTransferCancelledTitle, SharedResourcesKeys.NotificationTransferCancelledBody);

                    await _notificationService.SendNotificationToUserAsync(
                        request.RequestedBy.ApplicationUserId,
                        _localizer[titleKey].Value,
                        _localizer[bodyKey].Value,
                        id,
                        null,
                        null,
                        "Transfer");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "TransferRequest {RequestId} action {Action}: failed to notify requester {RequesterId}.",
                        id, actionLower, request.RequestedBy.Id);
                }
            }

            return await GetByIdAsync(request.Id);
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _db.TransferRequests.CountAsync(r => r.CreatedDate.Year == year);
            return $"TRF-{year}-{(count + 1):D5}";
        }

        private static GetDropDownStatusDto? MapStatus(Status? s) => s is null ? null : new GetDropDownStatusDto
        {
            Id = s.Id,
            nameEn = s.nameEn,
            nameAr = s.nameAr,
            Code = s.Code,
            orderNumber = s.orderNumber,
            iconName = s.iconName
        };

        private GetTransferRequestDto MapToDto(TransferRequest r) => new()
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestDate = r.RequestDate,
            SourceProjectId = r.SourceProjectId,
            SourceProject = r.SourceProject is null ? null : new GetProjectDto { Id = r.SourceProject.Id, nameEn = r.SourceProject.nameEn, nameAr = r.SourceProject.nameAr },
            SourceWarehouse = r.SourceWarehouse,
            DestinationProjectId = r.DestinationProjectId,
            DestinationProject = r.DestinationProject is null ? null : new GetProjectDto { Id = r.DestinationProject.Id, nameEn = r.DestinationProject.nameEn, nameAr = r.DestinationProject.nameAr },
            DestinationWarehouse = r.DestinationWarehouse,
            RequestedById = r.RequestedById,
            RequestedBy = r.RequestedBy is null ? null : new GetEngineerDto { Id = r.RequestedBy.Id, nameEn = r.RequestedBy.nameEn, nameAr = r.RequestedBy.nameAr },
            Notes = r.Notes,
            StatusId = r.StatusId,
            Status = MapStatus(r.Status),
            CreatedDate = r.CreatedDate,
            Items = r.Items.Select(i => new GetTransferRequestItemDto
            {
                Id = i.Id, ItemCode = i.ItemCode, ItemName = i.ItemName,
                Unit = i.Unit, Quantity = i.Quantity, ReceivedQuantity = i.ReceivedQuantity, Notes = i.Notes
            }).ToList(),
            Attachments = r.Attachments.Select(a => new GetAttachmentDto
            {
                Id = a.Id, Key = a.Key, FileName = a.FileName,
                Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url
            }).ToList(),
            Activities = r.Activities.Select(a => new GetTransferRequestActivityDto
            {
                Id = a.Id,
                FromStatusId = a.FromStatusId,
                FromStatus = MapStatus(a.FromStatus),
                ToStatusId = a.ToStatusId,
                ToStatus = MapStatus(a.ToStatus),
                ActionType = a.ActionType,
                Comments = a.Comments,
                Engineer = a.Engineer is null ? null : new GetEngineerDto { Id = a.Engineer.Id, nameEn = a.Engineer.nameEn, nameAr = a.Engineer.nameAr },
                CreatedDate = a.CreatedDate
            }).ToList(),
            EngineerRequestNotes = r.Activities
                .Where(a => !string.IsNullOrWhiteSpace(a.Comments))
                .Select(a => new GetEngineerRequestNotesDto
                {
                    Id          = a.Id,
                    note        = a.Comments,
                    EngineerId  = a.EngineerId,
                    Engineer    = a.Engineer is null ? null : new GetEngineerDto { Id = a.Engineer.Id, nameEn = a.Engineer.nameEn, nameAr = a.Engineer.nameAr },
                    CreatedDate = a.CreatedDate,
                    Attachments = new List<GetAttachmentDto>()
                })
                .OrderBy(n => n.CreatedDate)
                .ToList(),
            NeedsAcknowledgment = r.NeedsAcknowledgment
        };
    }
}
