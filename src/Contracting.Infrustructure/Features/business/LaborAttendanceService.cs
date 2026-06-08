using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.business.enums;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using ErrorOr;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.business
{
    public class LaborAttendanceService : ILaborAttendanceService
    {
        private readonly ApplicationDbContext _db;
        private readonly Storage.AWS3.Services.IStorageService _storageService;
        private readonly INotificationService _notificationService;

        public LaborAttendanceService(ApplicationDbContext db, Storage.AWS3.Services.IStorageService storageService, INotificationService notificationService)
        {
            _db = db;
            _storageService = storageService;
            _notificationService = notificationService;
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> CreateAsync(CreateLaborAttendanceRequestDto dto)
        {
            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            if (dto.Records == null || dto.Records.Count == 0)
                return Error.Validation("LaborAttendance.RecordsRequired", "At least one labor record must be added.");

            var request = new LaborAttendanceRequest
            {
                RequestNumber = await GenerateRequestNumberAsync(),
                ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId,
                DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId,
                SiteName = dto.SiteName,
                AttendanceDate = dto.AttendanceDate,
                SupervisorId = engineer?.Id,
                Notes = dto.Notes,
                Status = LaborAttendanceStatus.Draft
            };

            foreach (var rec in dto.Records)
            {
                if (!Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out var attendanceStatus))
                    return Error.Validation("LaborAttendance.InvalidStatus", $"Invalid attendance status: {rec.AttendanceStatus}");

                if (rec.DailyRate < 0)
                    return Error.Validation("LaborAttendance.NegativeRate", "Daily rate cannot be negative.");

                if (rec.OvertimeHours < 0)
                    return Error.Validation("LaborAttendance.NegativeOvertime", "Overtime hours cannot be negative.");

                if (attendanceStatus == WorkerAttendanceStatus.Absent && rec.OvertimeHours > 0)
                    return Error.Validation("LaborAttendance.AbsentWithOvertime", $"Worker '{rec.Name}' is absent but has overtime hours.");

                var totalAmount = CalculateTotalAmount(attendanceStatus, rec.DailyRate, rec.OvertimeHours);

                request.Records.Add(new LaborAttendanceRecord
                {
                    Name = rec.Name,
                    JobTitle = rec.JobTitle,
                    AttendanceStatus = attendanceStatus,
                    DailyRate = rec.DailyRate,
                    OvertimeHours = rec.OvertimeHours,
                    TotalAmount = totalAmount,
                    Notes = rec.Notes
                });
            }

            // Validate no duplicate names on same date
            var names = request.Records.Where(r => !string.IsNullOrWhiteSpace(r.Name))
                .Select(r => r.Name!.Trim().ToLower()).ToList();
            if (names.Count != names.Distinct().Count())
                return Error.Validation("LaborAttendance.DuplicateRecord", "Duplicate labor records are not allowed on the same request.");

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                    foreach (var f in uploaded)
                        request.Attachments.Add(new LaborAttendanceAttachment
                        {
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            request.Activities.Add(new LaborAttendanceActivity
            {
                EngineerId = engineer?.Id,
                ToStatus = LaborAttendanceStatus.Draft,
                ActionType = "Created"
            });

            await _db.LaborAttendanceRequests.AddAsync(request);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> UpdateAsync(UpdateLaborAttendanceRequestDto dto)
        {
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Records)
                .Include(r => r.Attachments)
                .FirstOrDefaultAsync(r => r.Id == dto.Id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            if (request.Status != LaborAttendanceStatus.Draft)
                return Error.Validation("LaborAttendance.CannotEdit", "Only Draft requests can be edited.");

            if (dto.ProjectId.HasValue) request.ProjectId = dto.ProjectId == Guid.Empty ? null : dto.ProjectId;
            if (dto.DepartmentId.HasValue) request.DepartmentId = dto.DepartmentId == Guid.Empty ? null : dto.DepartmentId;
            if (dto.SiteName is not null) request.SiteName = dto.SiteName;
            if (dto.AttendanceDate.HasValue) request.AttendanceDate = dto.AttendanceDate.Value;
            if (dto.Notes is not null) request.Notes = dto.Notes;

            if (dto.Records != null)
            {
                _db.LaborAttendanceRecords.RemoveRange(request.Records);
                foreach (var rec in dto.Records)
                {
                    if (!Enum.TryParse<WorkerAttendanceStatus>(rec.AttendanceStatus, true, out var attendanceStatus))
                        return Error.Validation("LaborAttendance.InvalidStatus", $"Invalid attendance status: {rec.AttendanceStatus}");
                    if (rec.DailyRate < 0)
                        return Error.Validation("LaborAttendance.NegativeRate", "Daily rate cannot be negative.");
                    if (rec.OvertimeHours < 0)
                        return Error.Validation("LaborAttendance.NegativeOvertime", "Overtime hours cannot be negative.");
                    if (attendanceStatus == WorkerAttendanceStatus.Absent && rec.OvertimeHours > 0)
                        return Error.Validation("LaborAttendance.AbsentWithOvertime", $"Worker '{rec.Name}' is absent but has overtime hours.");

                    request.Records.Add(new LaborAttendanceRecord
                    {
                        LaborAttendanceRequestId = request.Id,
                        Name = rec.Name,
                        JobTitle = rec.JobTitle,
                        AttendanceStatus = attendanceStatus,
                        DailyRate = rec.DailyRate,
                        OvertimeHours = rec.OvertimeHours,
                        TotalAmount = CalculateTotalAmount(attendanceStatus, rec.DailyRate, rec.OvertimeHours),
                        Notes = rec.Notes
                    });
                }
            }

            if (dto.Attachments != null && dto.Attachments.Any())
            {
                var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
                if (uploaded != null)
                    foreach (var f in uploaded)
                        request.Attachments.Add(new LaborAttendanceAttachment
                        {
                            LaborAttendanceRequestId = request.Id,
                            Key = f.Key, FileName = f.FileName, Extension = f.Extension,
                            FileSize = f.FileSize, Url = f.Url
                        });
            }

            await _db.SaveChangesAsync();
            return await GetByIdAsync(request.Id);
        }

        public async Task<ErrorOr<GenericResponse>> DeleteAsync(Guid id)
        {
            var request = await _db.LaborAttendanceRequests.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            if (request.Status != LaborAttendanceStatus.Draft)
                return Error.Validation("LaborAttendance.CannotDelete", "Only Draft requests can be deleted.");

            request.MarkAsDeleted(CurrentUser.Id ?? Guid.Empty);
            await _db.SaveChangesAsync();
            return GenericResponse.SuccessResult("Deleted successfully.");
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> GetByIdAsync(Guid id)
        {
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Supervisor)
                .Include(r => r.AssignedTo)
                .Include(r => r.Records)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");
            return MapToDto(request);
        }

        public async Task<PaginatedList<GetLaborAttendanceRequestDto>> GetAllAsync(LaborAttendanceFilterDto filter)
        {
            var query = _db.LaborAttendanceRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Supervisor)
                .Include(r => r.AssignedTo)
                .Include(r => r.Records)
                .Include(r => r.Attachments)
                .Include(r => r.Activities).ThenInclude(a => a.Engineer)
                .Where(r => !r.IsDeleted)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(filter.Status)
                && Enum.TryParse<LaborAttendanceStatus>(filter.Status, true, out var statusEnum))
                query = query.Where(r => r.Status == statusEnum);

            if (filter.ProjectId.HasValue) query = query.Where(r => r.ProjectId == filter.ProjectId);
            if (filter.SupervisorId.HasValue) query = query.Where(r => r.SupervisorId == filter.SupervisorId);
            if (filter.FromDate.HasValue) query = query.Where(r => r.AttendanceDate >= filter.FromDate);
            if (filter.ToDate.HasValue) query = query.Where(r => r.AttendanceDate <= filter.ToDate);
            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(r => r.RequestNumber!.Contains(filter.Search) || r.SiteName!.Contains(filter.Search));

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(r => r.CreatedDate)
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PaginatedList<GetLaborAttendanceRequestDto>(
                items.Select(MapToDto).ToList(),
                totalCount, filter.PageIndex, filter.PageSize);
        }

        public async Task<ErrorOr<GetLaborAttendanceRequestDto>> TakeActionAsync(Guid id, LaborAttendanceActionDto dto)
        {
            var request = await _db.LaborAttendanceRequests
                .Include(r => r.Records)
                .Include(r => r.Activities)
                .Include(r => r.Supervisor)
                .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

            if (request is null) return Error.NotFound("LaborAttendance.NotFound", "Labor attendance request not found.");

            var engineer = await _db.Engineers.AsNoTracking()
                .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!));

            Domain.Entities.master.Engineer? assignedEngineer = null;
            var fromStatus = request.Status;
            LaborAttendanceStatus toStatus;

            switch (dto.ActionType.ToLower())
            {
                case "submit":
                    if (request.Status != LaborAttendanceStatus.Draft)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Draft requests can be submitted.");
                    if (!request.Records.Any())
                        return Error.Validation("LaborAttendance.NoRecords", "Cannot submit a request with no labor records.");
                    toStatus = LaborAttendanceStatus.Pending;
                    break;
                case "assign":
                    if (request.Status != LaborAttendanceStatus.Pending)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Pending requests can be assigned.");
                    if (!dto.AssignedToId.HasValue || dto.AssignedToId == Guid.Empty)
                        return Error.Validation("LaborAttendance.AssignedToRequired", "AssignedToId is required for assign action.");

                    assignedEngineer = await _db.Engineers.AsNoTracking()
                        .FirstOrDefaultAsync(e => e.Id == dto.AssignedToId.Value);
                    if (assignedEngineer is null)
                        return Error.NotFound("LaborAttendance.EngineerNotFound", "Assigned engineer not found.");

                    request.AssignedToId = dto.AssignedToId.Value;
                    toStatus = LaborAttendanceStatus.Submitted;
                    break;
                case "validate":
                    if (request.Status != LaborAttendanceStatus.Submitted)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Submitted requests can be validated.");
                    toStatus = LaborAttendanceStatus.Validated;
                    break;
                case "reject":
                    if (request.Status != LaborAttendanceStatus.Submitted)
                        return Error.Validation("LaborAttendance.InvalidAction", "Only Submitted requests can be rejected.");
                    toStatus = LaborAttendanceStatus.Rejected;
                    break;
                default:
                    return Error.Validation("LaborAttendance.UnknownAction", $"Unknown action: {dto.ActionType}");
            }

            request.Status = toStatus;
            request.Activities.Add(new LaborAttendanceActivity
            {
                LaborAttendanceRequestId = request.Id,
                EngineerId = engineer?.Id,
                FromStatus = fromStatus,
                ToStatus = toStatus,
                ActionType = dto.ActionType,
                Comments = dto.Comments
            });

            await _db.SaveChangesAsync();

            // Send notifications after save
            var supervisorUserId = request.Supervisor?.ApplicationUserId;
            switch (dto.ActionType.ToLower())
            {
                case "assign":
                    if (assignedEngineer is not null)
                        await _notificationService.SendNotificationToUserAsync(
                            assignedEngineer.ApplicationUserId,
                            "Labor Attendance Request Assigned",
                            $"Request {request.RequestNumber} has been assigned to you for review.",
                            request.Id);
                    break;
                case "validate":
                    if (supervisorUserId.HasValue)
                        await _notificationService.SendNotificationToUserAsync(
                            supervisorUserId.Value,
                            "Labor Attendance Request Validated",
                            $"Your request {request.RequestNumber} has been validated.",
                            request.Id);
                    break;
                case "reject":
                    if (supervisorUserId.HasValue)
                        await _notificationService.SendNotificationToUserAsync(
                            supervisorUserId.Value,
                            "Labor Attendance Request Rejected",
                            $"Your request {request.RequestNumber} has been rejected. {dto.Comments}",
                            request.Id);
                    break;
            }

            return await GetByIdAsync(request.Id);
        }

        private static decimal CalculateTotalAmount(WorkerAttendanceStatus status, decimal dailyRate, decimal overtimeHours)
        {
            return status switch
            {
                WorkerAttendanceStatus.Present => dailyRate,
                WorkerAttendanceStatus.HalfDay => dailyRate * 0.5m,
                WorkerAttendanceStatus.Overtime => dailyRate + (overtimeHours * (dailyRate / 8m)),
                WorkerAttendanceStatus.Absent => 0m,
                _ => 0m
            };
        }

        private async Task<string> GenerateRequestNumberAsync()
        {
            var year = DateTime.UtcNow.Year;
            var count = await _db.LaborAttendanceRequests.CountAsync(r => r.CreatedDate.Year == year);
            return $"LAB-{year}-{(count + 1):D5}";
        }

        private static GetLaborAttendanceRequestDto MapToDto(LaborAttendanceRequest r) => new()
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            ProjectId = r.ProjectId,
            Project = r.Project is null ? null : new GetProjectDto { Id = r.Project.Id, nameEn = r.Project.nameEn, nameAr = r.Project.nameAr },
            DepartmentId = r.DepartmentId,
            Department = r.Department is null ? null : new GetDepartmentDto { Id = r.Department.Id, nameEn = r.Department.nameEn, nameAr = r.Department.nameAr },
            SiteName = r.SiteName,
            AttendanceDate = r.AttendanceDate,
            SupervisorId = r.SupervisorId,
            Supervisor = r.Supervisor is null ? null : new GetEngineerDto { Id = r.Supervisor.Id, nameEn = r.Supervisor.nameEn, nameAr = r.Supervisor.nameAr },
            AssignedToId = r.AssignedToId,
            AssignedTo = r.AssignedTo is null ? null : new GetEngineerDto { Id = r.AssignedTo.Id, nameEn = r.AssignedTo.nameEn, nameAr = r.AssignedTo.nameAr },
            Notes = r.Notes,
            Status = r.Status.ToString(),
            TotalAmount = r.Records.Sum(rec => rec.TotalAmount),
            CreatedDate = r.CreatedDate,
            Records = r.Records.Select(rec => new GetLaborAttendanceRecordDto
            {
                Id = rec.Id,
                Name = rec.Name,
                JobTitle = rec.JobTitle,
                AttendanceStatus = rec.AttendanceStatus.ToString(),
                DailyRate = rec.DailyRate,
                OvertimeHours = rec.OvertimeHours,
                TotalAmount = rec.TotalAmount,
                Notes = rec.Notes
            }).ToList(),
            Attachments = r.Attachments.Select(a => new GetAttachmentDto
            {
                Id = a.Id, Key = a.Key, FileName = a.FileName,
                Extension = a.Extension, FileSize = a.FileSize, Url = a.Url
            }).ToList(),
            Activities = r.Activities.Select(a => new GetLaborAttendanceActivityDto
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
