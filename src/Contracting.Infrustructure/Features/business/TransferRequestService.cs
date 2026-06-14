using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.business.enums;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business
{
    public class TransferRequestService : ITransferRequestService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;

        public TransferRequestService(ApplicationDbContext db, Storage.AWS3.Services.IStorageService storageService)
        {
            _db = db;
            _storageService = storageService;
        }

        public async Task<ErrorOr<GetTransferRequestDto>> CreateAsync(CreateTransferRequestDto dto)
        {
            var engineer = await _db.Engineers
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            // Validation: Destination must be selected
            if (dto.DestinationProjectId == null && string.IsNullOrWhiteSpace(dto.DestinationWarehouse))
                return Error.Validation("TransferRequest.DestinationRequired", "Destination project or warehouse must be selected.");

            // Validation: Must have at least one item
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
                Status = TransferRequestStatus.PendingReceipt
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
                ToStatus = TransferRequestStatus.PendingReceipt,
                ActionType = "Submitted"
            });

            await _db.TransferRequests.AddAsync(request);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GetTransferRequestDto>> UpdateAsync(UpdateTransferRequestDto dto)
        {
            var request = await _db.TransferRequests
                .Include(r => r.Items)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");
            if (request.Status != TransferRequestStatus.PendingReceipt)
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
            var request = await _db.TransferRequests.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");
            if (request.Status != TransferRequestStatus.PendingReceipt)
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
                .Include(r => r.Items)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
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
                .Include(r => r.Items)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
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
            }

            if (!string.IsNullOrWhiteSpace(filter.Status)
                && Enum.TryParse<TransferRequestStatus>(filter.Status, true, out var statusEnum))
                query = query.Where(r => r.Status == statusEnum);

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
            var request = await _db.TransferRequests
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("TransferRequest.NotFound", "Transfer request not found.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            var fromStatus = request.Status;
            TransferRequestStatus toStatus;

            switch (dto.ActionType.ToLower())
            {
                case "submit":
                    if (request.Status != TransferRequestStatus.Draft)
                        return Error.Validation("TransferRequest.InvalidAction", "Only Draft requests can be submitted.");
                    toStatus = TransferRequestStatus.PendingReceipt;
                    break;
                case "confirmreceipt":
                    if (request.Status != TransferRequestStatus.PendingReceipt && request.Status != TransferRequestStatus.PartiallyReceived)
                        return Error.Validation("TransferRequest.InvalidAction", "Request must be in PendingReceipt or PartiallyReceived status.");
                    toStatus = TransferRequestStatus.Closed;
                    break;
                case "confirmpartialreceipt":
                    if (request.Status != TransferRequestStatus.PendingReceipt)
                        return Error.Validation("TransferRequest.InvalidAction", "Request must be in PendingReceipt status.");
                    toStatus = TransferRequestStatus.PartiallyReceived;
                    break;
                case "cancel":
                    if (request.Status == TransferRequestStatus.Closed)
                        return Error.Validation("TransferRequest.InvalidAction", "Closed requests cannot be cancelled.");
                    toStatus = TransferRequestStatus.Cancelled;
                    break;
                default:
                    return Error.Validation("TransferRequest.UnknownAction", $"Unknown action: {dto.ActionType}");
            }

            // Use direct SQL update to avoid EF Core concurrency tracking issues
            await _db.TransferRequests
                .Where(r => r.Id == id)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(r => r.Status, toStatus)
                    .SetProperty(r => r.ModifiedDate, DateTimeOffset.UtcNow)
                    .SetProperty(r => r.ModifiedBy, engineer != null ? engineer.Id : (Guid?)null));

            // Insert the activity log
            await _db.TransferRequestActivities.AddAsync(new TransferRequestActivity
            {
                TransferRequestId = id,
                EngineerId = engineer?.Id,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ActionType = dto.ActionType,
                Comments = dto.Comments
            });

            await _db.SaveChangesAsync();
            return await GetByIdAsync(request.Id);
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _db.TransferRequests.CountAsync(r => r.CreatedDate.Year == year);
            return $"TRF-{year}-{(count + 1):D5}";
        }

        private static GetTransferRequestDto MapToDto(TransferRequest r) => new()
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
            Status = r.Status.ToString(),
            CreatedDate = r.CreatedDate,
            Items = r.Items.Select(i => new GetTransferRequestItemDto
            {
                Id = i.Id, ItemCode = i.ItemCode, ItemName = i.ItemName,
                Unit = i.Unit, Quantity = i.Quantity, Notes = i.Notes
            }).ToList(),
            Attachments = r.Attachments.Select(a => new GetAttachmentDto
            {
                Id = a.Id, Key = a.Key, FileName = a.FileName,
                Extension = a.Extension, FileSize = a.FileSize, Url = a.Url
            }).ToList(),
            Activities = r.Activities.Select(a => new GetTransferRequestActivityDto
            {
                Id = a.Id,
                FromStatus = a.FromStatus?.ToString(),
                ToStatus = a.ToStatus.ToString(),
                ActionType = a.ActionType,
                Comments = a.Comments,
                Engineer = a.Engineer is null ? null : new GetEngineerDto { Id = a.Engineer.Id, nameEn = a.Engineer.nameEn, nameAr = a.Engineer.nameAr },
                CreatedDate = a.CreatedDate
            }).ToList()
        };
    }
}
