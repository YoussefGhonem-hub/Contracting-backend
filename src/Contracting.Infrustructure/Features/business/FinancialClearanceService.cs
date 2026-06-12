using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.business.enums;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business
{
    public class FinancialClearanceService : IFinancialClearanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;

        public FinancialClearanceService(ApplicationDbContext db, Storage.AWS3.Services.IStorageService storageService)
        {
            _db = db;
            _storageService = storageService;
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> CreateAsync(CreateFinancialClearanceDto dto)
        {
            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            // Validation: spent cannot exceed advance
            if (dto.SpentAmount > dto.AdvanceAmount)
                return Error.Validation("FinancialClearance.SpentExceedsAdvance", "Spent amount cannot exceed advance amount.");

            var clearance = new FinancialClearance
            {
                ClearanceNumber = await GenerateClearanceNumberAsync(),
                EmployeeName = dto.EmployeeName,
                DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId,
                ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId,
                RequestDate = dto.RequestDate,
                AdvanceAmount = dto.AdvanceAmount,
                SpentAmount = dto.SpentAmount,
                RemainingAmount = dto.AdvanceAmount - dto.SpentAmount,
                Notes = dto.Notes,
                Status = FinancialClearanceStatus.Draft,
                RequestedById = engineer?.Id
            };

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var files = dto.Attachments.ToList();
                var uploaded = await _storageService.UploadFiles(files);
                for (int i = 0; i < (uploaded?.Count ?? 0); i++)
                {
                    var f = uploaded![i];
                    clearance.Attachments.Add(new FinancialClearanceAttachment
                    {
                        Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                        FileSize = f.FileSize, Url = f.Url,
                        AttachmentType = dto.AttachmentTypes != null && i < dto.AttachmentTypes.Count
                            ? dto.AttachmentTypes[i] : null
                    });
                }
            }

            clearance.Activities.Add(new FinancialClearanceActivity
            {
                EngineerId = engineer?.Id,
                ToStatus = FinancialClearanceStatus.Draft,
                ActionType = "Created"
            });

            await _db.FinancialClearances.AddAsync(clearance);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(clearance.Id);
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> UpdateAsync(UpdateFinancialClearanceDto dto)
        {
            var clearance = await _db.FinancialClearances
                .Include(c => c.Attachments)
                .FirstOrDefaultAsync(c => c.Id == dto.Id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");
            if (clearance.Status != FinancialClearanceStatus.Draft)
                return Error.Validation("FinancialClearance.CannotEdit", "Only Draft clearances can be edited.");

            if (dto.EmployeeName is not null) clearance.EmployeeName = dto.EmployeeName;
            if (dto.DepartmentId.HasValue) clearance.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            if (dto.ProjectId.HasValue) clearance.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            if (dto.RequestDate.HasValue) clearance.RequestDate = dto.RequestDate.Value;
            if (dto.Notes is not null) clearance.Notes = dto.Notes;

            var advanceAmount = dto.AdvanceAmount ?? clearance.AdvanceAmount;
            var spentAmount = dto.SpentAmount ?? clearance.SpentAmount;

            if (spentAmount > advanceAmount)
                return Error.Validation("FinancialClearance.SpentExceedsAdvance", "Spent amount cannot exceed advance amount.");

            clearance.AdvanceAmount = advanceAmount;
            clearance.SpentAmount = spentAmount;
            clearance.RemainingAmount = advanceAmount - spentAmount;

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var files = dto.Attachments.ToList();
                var uploaded = await _storageService.UploadFiles(files);
                for (int i = 0; i < (uploaded?.Count ?? 0); i++)
                {
                    var f = uploaded![i];
                    clearance.Attachments.Add(new FinancialClearanceAttachment
                    {
                        FinancialClearanceId = clearance.Id,
                        Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                        FileSize = f.FileSize, Url = f.Url,
                        AttachmentType = dto.AttachmentTypes != null && i < dto.AttachmentTypes.Count
                            ? dto.AttachmentTypes[i] : null
                    });
                }
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(clearance.Id);
        }

        public async Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id)
        {
            var clearance = await _db.FinancialClearances.FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);
            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");
            if (clearance.Status != FinancialClearanceStatus.Draft)
                return Error.Validation("FinancialClearance.CannotDelete", "Only Draft clearances can be deleted.");

            clearance.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult("Deleted successfully.");
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> GetByIdAsync(Guid id)
        {
            var clearance = await _db.FinancialClearances
                .Include(c => c.Department)
                .Include(c => c.Project)
                .Include(c => c.RequestedBy)
                .Include(c => c.Attachments)
                .Include(c => c.Activities).ThenInclude(a => a.Engineer)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");
            return MapToDto(clearance);
        }

        public async Task<PaginatedList<GetFinancialClearanceDto>> GetAllAsync(FinancialClearanceFilterDto filter)
        {
            var query = _db.FinancialClearances
                .Include(c => c.Department)
                .Include(c => c.Project)
                .Include(c => c.RequestedBy)
                .Include(c => c.Attachments)
                .Include(c => c.Activities).ThenInclude(a => a.Engineer)
                .Where(c => !c.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Status)
                && Enum.TryParse<FinancialClearanceStatus>(filter.Status, true, out var statusEnum))
                query = query.Where(c => c.Status == statusEnum);

            if (filter.ProjectId.HasValue) query = query.Where(c => c.ProjectId == filter.ProjectId);
            if (filter.DepartmentId.HasValue) query = query.Where(c => c.DepartmentId == filter.DepartmentId);
            if (filter.RequestedById.HasValue) query = query.Where(c => c.RequestedById == filter.RequestedById);
            if (filter.FromDate.HasValue) query = query.Where(c => c.RequestDate >= filter.FromDate);
            if (filter.ToDate.HasValue) query = query.Where(c => c.RequestDate <= filter.ToDate);
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(c => c.ClearanceNumber!.Contains(filter.Search) || c.EmployeeName!.Contains(filter.Search));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(c => c.CreatedDate)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedList<GetFinancialClearanceDto>(
                items.Select(MapToDto).ToList(),
                totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<ErrorOr<GetFinancialClearanceDto>> TakeActionAsync(Guid id, FinancialClearanceActionDto dto)
        {
            var clearance = await _db.FinancialClearances
                .Include(c => c.Attachments)
                .Include(c => c.Activities)
                .FirstOrDefaultAsync(c => c.Id == id && !c.IsDeleted);

            if (clearance is null) return Error.NotFound("FinancialClearance.NotFound", "Financial clearance not found.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            var fromStatus = clearance.Status;
            FinancialClearanceStatus toStatus;

            switch (dto.ActionType)
            {
                case FinancialClearanceActionType.Submit:
                    if (clearance.Status != FinancialClearanceStatus.Draft)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only Draft clearances can be submitted.");
                    toStatus = FinancialClearanceStatus.Submitted;
                    break;
                case FinancialClearanceActionType.Approve:
                    if (clearance.Status != FinancialClearanceStatus.Submitted)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only Submitted clearances can be approved.");
                    toStatus = FinancialClearanceStatus.Approved;
                    break;
                case FinancialClearanceActionType.Close:
                    if (clearance.Status != FinancialClearanceStatus.Approved)
                        return Error.Validation("FinancialClearance.InvalidAction", "Only Approved clearances can be closed.");
                    if (!clearance.Attachments.Any())
                        return Error.Validation("FinancialClearance.MissingAttachments", "Attachments are required before closing.");
                    toStatus = FinancialClearanceStatus.Closed;
                    break;
                case FinancialClearanceActionType.Reject:
                    if (clearance.Status == FinancialClearanceStatus.Closed || clearance.Status == FinancialClearanceStatus.Draft)
                        return Error.Validation("FinancialClearance.InvalidAction", "Cannot reject a closed or draft clearance.");
                    toStatus = FinancialClearanceStatus.Rejected;
                    break;
                default:
                    return Error.Validation("FinancialClearance.UnknownAction", $"Unknown action: {dto.ActionType}");
            }

            clearance.Status = toStatus;
            clearance.Activities.Add(new FinancialClearanceActivity
            {
                FinancialClearanceId = clearance.Id,
                EngineerId = engineer?.Id,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ActionType = dto.ActionType.ToString(),
                Comments = dto.Comments
            });

            await _db.SaveChangesAsync();
            return await GetByIdAsync(clearance.Id);
        }

        private async Task<string> GenerateClearanceNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _db.FinancialClearances.CountAsync(c => c.CreatedDate.Year == year);
            return $"FIN-{year}-{(count + 1):D5}";
        }

        private static GetFinancialClearanceDto MapToDto(FinancialClearance c) => new()
        {
            Id = c.Id,
            ClearanceNumber = c.ClearanceNumber,
            EmployeeName = c.EmployeeName,
            DepartmentId = c.DepartmentId,
            Department = c.Department is null ? null : new GetDepartmentDto { Id = c.Department.Id, nameEn = c.Department.nameEn, nameAr = c.Department.nameAr },
            ProjectId = c.ProjectId,
            Project = c.Project is null ? null : new GetProjectDto { Id = c.Project.Id, nameEn = c.Project.nameEn, nameAr = c.Project.nameAr },
            RequestDate = c.RequestDate,
            AdvanceAmount = c.AdvanceAmount,
            SpentAmount = c.SpentAmount,
            RemainingAmount = c.RemainingAmount,
            Notes = c.Notes,
            Status = c.Status.ToString(),
            RequestedById = c.RequestedById,
            RequestedBy = c.RequestedBy is null ? null : new GetEngineerDto { Id = c.RequestedBy.Id, nameEn = c.RequestedBy.nameEn, nameAr = c.RequestedBy.nameAr },
            CreatedDate = c.CreatedDate,
            Attachments = c.Attachments.Select(a => new GetFinancialClearanceAttachmentDto
            {
                Id = a.Id, Key = a.Key, FileName = a.FileName,
                Extension = a.Extension, FileSize = a.FileSize, Url = a.Url,
                AttachmentType = a.AttachmentType
            }).ToList(),
            Activities = c.Activities.Select(a => new GetFinancialClearanceActivityDto
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
