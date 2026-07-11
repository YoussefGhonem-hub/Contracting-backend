using Contracting.Shared.Resources;
using Contracting.Domain.Entities.business;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Extensions;
using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Domain.Entities.business.enums;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.PurchaseRequestDto;
using Contracting.Shared.BusinessDtos.UnifiedRequestDto;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using Contracting.Shared.Common;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using Contracting.Shared.Dtos.MasterDtos.ConstructionItemDtos;
using ErrorOr;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Storage.AWS3.Services;
using Storage.AWS3.Models;
using Microsoft.Extensions.Logging;
namespace Contracting.Infrustructure.Features.business;



public class EngineerRequestService : IEngineerRequestService
{
    private readonly ApplicationDbContext _db;
    private readonly IMapper _mapper;
    private readonly INotificationService _notificationService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IStorageService _storageService;
    private readonly ILogger<EngineerRequestService> _logger;
    private static readonly string[] InProgressKeywords = { "in progress", "progress", "processing", "working" };
    private static readonly string[] DelayedKeywords = { "delay", "delayed", "late", "overdue" };
    private static readonly string[] CompletedKeywords = { "completed", "complete", "done", "finished", "finish", "closed" };
    private static readonly string[] NewPendingKeywords = { "new", "pending", "open", "submitted", "created" };
    private static readonly string[] RejectedKeywords = { "rejected", "reject", "denied", "deny" };
    private static readonly string[] PendingInfoKeywords = { "missing information", "missing info", "missing_information" };

    private static bool StatusMatchesKeywords(Contracting.Domain.Entities.master.Status status, string[] keywords)
        => keywords.Any(k =>
            (status.Code?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameEn?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameAr?.Contains(k, StringComparison.OrdinalIgnoreCase) == true));


    public EngineerRequestService(ApplicationDbContext db, IMapper mapper, INotificationService notificationService, IStringLocalizer<SharedResources> localizer, IStorageService storageService, ILogger<EngineerRequestService> logger)
    {
        _db = db;
        _mapper = mapper;
        _notificationService = notificationService;
        _localizer = localizer;
        _storageService = storageService;
        _logger = logger;
    }

    // ---------------- CREATE ----------------
    public async Task<ErrorOr<GetAllEngineerRequestDto>> CreateEngineerRequestAsync(CreateEngineerRequestDto dto)
    {
        var roles = CurrentUser.Roles;
        var isSiteEngineer = roles.Any(r => r.Equals(RoleNames.Siteengineer, StringComparison.OrdinalIgnoreCase));
        if (!isSiteEngineer)
            return Error.Forbidden("Auth.Forbidden", _localizer[SharedResourcesKeys.SiteEngineerOnlyCreateRequest]);

        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        var request = _mapper.Map<EngineerRequest>(dto);
        request.EngineerId = engineer?.Id;

        var firstStatus = await _db.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.orderNumber == 1);

        request.StatusId = firstStatus?.Id ?? Guid.Empty;

        // Handle empty GUIDs
        if (request.ProjectId == Guid.Empty) request.ProjectId = null;
        if (request.DepartmentId == Guid.Empty) request.DepartmentId = null;
        if (request.PriorityId == Guid.Empty) request.PriorityId = null;
        if (request.EngineerId == Guid.Empty) request.EngineerId = null;

        // Find the Team Lead for the selected department (via EngineerDepartments first, then legacy)
        var teamLead = await _db.EngineerDepartments
            .Where(ed => ed.DepartmentId == request.DepartmentId && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
            .Select(ed => ed.Engineer!.ApplicationUserId)
            .FirstOrDefaultAsync();

        if (teamLead == Guid.Empty)
        {
            teamLead = await (from eng in _db.Engineers
                          join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                          join role in _db.Roles on userRole.RoleId equals role.Id
                          where eng.DepartmentId == request.DepartmentId && role.Name == RoleNames.Teamleadengineer
                          select eng.ApplicationUserId)
                  .FirstOrDefaultAsync();
        }

        // Handle notes (and their attachments)
        if (dto.EngineerRequestNotes != null && dto.EngineerRequestNotes.Any())
        {
            request.EngineerRequestNotes = new List<EngineerRequestNotes>();
            foreach (var noteDto in dto.EngineerRequestNotes)
            {
                var note = _mapper.Map<EngineerRequestNotes>(noteDto);
                note.EngineerId = engineer?.Id;
                note.EngineerRequest = request;

                // Handle note attachments
                if (noteDto.Attachments != null && noteDto.Attachments.Any())
                {
                    var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                    note.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
                    {
                        Key = f.Key,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        FileSize = f.FileSize,
                        Url = f.Url
                    }).ToList();
                }

                request.EngineerRequestNotes.Add(note);
            }
        }

        // Handle request-level attachments
        if (dto.Attachments != null && dto.Attachments.Any())
        {
            var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
            request.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
            {
                Key = f.Key,
                FileName = f.FileName,
                Extension = f.Extension,
                FileSize = f.FileSize,
                Url = f.Url
            }).ToList();
        }

        // Validate ConstructionItem special fields before persisting
        var createValidationError = await ValidateConstructionItemFieldsAsync(dto.SpecialFieldValues);
        if (createValidationError.HasValue) return createValidationError.Value;

        // Validate all DepartmentSpecialFieldIds in SpecialFieldValues
        if (dto.SpecialFieldValues != null && dto.SpecialFieldValues.Any())
        {
            var sfvFieldIds = dto.SpecialFieldValues.Select(s => s.DepartmentSpecialFieldId).Distinct().ToList();
            var existingSfvFieldIds = await _db.DepartmentSpecialFields
                .Where(dsf => sfvFieldIds.Contains(dsf.Id))
                .Select(dsf => dsf.Id)
                .ToListAsync();
            var missingSfvFieldId = sfvFieldIds.Except(existingSfvFieldIds).FirstOrDefault();
            if (missingSfvFieldId != Guid.Empty)
                return Error.NotFound("DepartmentSpecialField.NotFound", _localizer[SharedResourcesKeys.NotFound]);
        }

        // Validate all DepartmentSpecialFieldIds in SpecialFieldItems
        if (dto.SpecialFieldItems != null && dto.SpecialFieldItems.Any())
        {
            var sfiFieldIds = dto.SpecialFieldItems.Select(i => i.DepartmentSpecialFieldId).Distinct().ToList();
            var existingSfiFieldIds = await _db.DepartmentSpecialFields
                .Where(dsf => sfiFieldIds.Contains(dsf.Id))
                .Select(dsf => dsf.Id)
                .ToListAsync();
            var missingSfiFieldId = sfiFieldIds.Except(existingSfiFieldIds).FirstOrDefault();
            if (missingSfiFieldId != Guid.Empty)
                return Error.NotFound("DepartmentSpecialField.NotFound", _localizer[SharedResourcesKeys.NotFound]);
        }

        // Validate all DepartmentSpecialFieldIds in SpecialFieldListItems
        if (dto.SpecialFieldListItems != null && dto.SpecialFieldListItems.Any())
        {
            var sfliFieldIds = dto.SpecialFieldListItems.Select(i => i.DepartmentSpecialFieldId).Distinct().ToList();
            var existingSfliFields = await _db.DepartmentSpecialFields
                .Where(dsf => sfliFieldIds.Contains(dsf.Id))
                .Select(dsf => new { dsf.Id, dsf.ListGroupKey })
                .ToListAsync();
            var missingSfliFieldId = sfliFieldIds.Except(existingSfliFields.Select(f => f.Id)).FirstOrDefault();
            if (missingSfliFieldId != Guid.Empty)
                return Error.NotFound("DepartmentSpecialField.NotFound", _localizer[SharedResourcesKeys.NotFound]);
            if (existingSfliFields.Any(f => string.IsNullOrWhiteSpace(f.ListGroupKey)))
                return Error.Validation("SpecialField.NotList", "This field is not configured as a repeatable list.");
        }

        await _db.EngineerRequests.AddAsync(request);
        
        // Save special field values
        if (dto.SpecialFieldValues != null && dto.SpecialFieldValues.Any())
        {
            foreach (var sfv in dto.SpecialFieldValues)
            {
                var specialFieldValue = new EngineerRequestSpecialFieldValue
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = sfv.DepartmentSpecialFieldId,
                    value = sfv.value
                };
                await _db.EngineerRequestSpecialFieldValues.AddAsync(specialFieldValue);
            }
        }

        // Save special field items (ConstructionItem + Quantity list for Procurement)
        if (dto.SpecialFieldItems != null && dto.SpecialFieldItems.Any())
        {
            // Validate all ConstructionItem IDs exist
            var itemIds = dto.SpecialFieldItems.Select(i => i.ConstructionItemId).Distinct().ToList();
            var existingItemIds = await _db.ConstructionItems
                .Where(ci => itemIds.Contains(ci.Id))
                .Select(ci => ci.Id)
                .ToListAsync();
            var missingItemId = itemIds.Except(existingItemIds).FirstOrDefault();
            if (missingItemId != Guid.Empty)
                return Error.NotFound("ConstructionItem.NotFound", _localizer[SharedResourcesKeys.ConstructionItemNotFound]);

            foreach (var item in dto.SpecialFieldItems)
            {
                await _db.EngineerRequestSpecialFieldItems.AddAsync(new EngineerRequestSpecialFieldItem
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = item.DepartmentSpecialFieldId,
                    ConstructionItemId = item.ConstructionItemId,
                    Quantity = item.Quantity
                });
            }
        }

        // Save special field list items (repeatable row values for list-flagged fields)
        if (dto.SpecialFieldListItems != null && dto.SpecialFieldListItems.Any())
        {
            foreach (var item in dto.SpecialFieldListItems)
            {
                await _db.EngineerRequestSpecialFieldListItems.AddAsync(new EngineerRequestSpecialFieldListItem
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = item.DepartmentSpecialFieldId,
                    RowIndex = item.RowIndex,
                    value = item.value
                });
            }
        }

        // Create initial activity for request creation
        var createActivity = new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = engineer?.Id,
            StatusId = request.StatusId,
            ActionType = EngineerRequestActionType.Created.ToString()
        };
        await _db.EngineerRequestActivites.AddAsync(createActivity);
        
        await _db.SaveChangesAsync();

        // Reload with navigation properties (including attachments)
        var createdRequest = await _db.EngineerRequests
                            .Include(r => r.Project)
                            .Include(r => r.Department)
                            .Include(r => r.Priority)
                            .Include(r => r.Status)
                            .Include(r => r.Engineer)
                                .ThenInclude(e => e.Department)
                            .Include(r => r.EngineerRequestNotes)
                                .ThenInclude(n => n.EngineerRequestAttachments)
                            .Include(r => r.EngineerRequestNotes)
                                .ThenInclude(n => n.Engineer)
                            .Include(r => r.EngineerRequestAttachments)
                            .Include(r=>r.EngineerRequestActivites)
                            .Include(r => r.SpecialFieldValues)
                                .ThenInclude(v => v.DepartmentSpecialField)
                                    .ThenInclude(psf => psf.SpecialField)
                            .Include(r => r.SpecialFieldItems)
                                .ThenInclude(i => i.ConstructionItem)
                                    .ThenInclude(c => c.Units)
                            .Include(r => r.SpecialFieldListItems)
                                .ThenInclude(i => i.DepartmentSpecialField)
                                    .ThenInclude(psf => psf.SpecialField)
                            .AsSplitQuery()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(r => r.Id == request.Id);


        if (teamLead == Guid.Empty)
        {
            // No team lead found, notify all engineers in the department (via EngineerDepartments + legacy)
            var departmentEngineerIds = await _db.EngineerDepartments
                .Where(ed => ed.DepartmentId == request.DepartmentId)
                .Select(ed => ed.Engineer!.ApplicationUserId)
                .ToListAsync();

            var legacyEngineerIds = await _db.Engineers
                .Where(e => e.DepartmentId == request.DepartmentId)
                .Select(e => e.ApplicationUserId)
                .ToListAsync();

            var allEngineerUserIds = departmentEngineerIds.Union(legacyEngineerIds).Distinct();
            foreach (var engineerUserId in allEngineerUserIds)
            {
                await _notificationService.SendNotificationToUserAsync(
                    engineerUserId,
                    _localizer[SharedResourcesKeys.NotificationNewRequestTitle],
                    _localizer[SharedResourcesKeys.NotificationNewRequestBody],
                    request.Id,
                    request.DepartmentId);
            }
        }
        else
        {
            // Notify only the team lead
            await _notificationService.SendNotificationToUserAsync(
                teamLead,
                _localizer[SharedResourcesKeys.NotificationNewRequestTitle],
                _localizer[SharedResourcesKeys.NotificationNewRequestBody],
                request.Id,
                request.DepartmentId);
        }

        var createdDto = _mapper.Map<GetAllEngineerRequestDto>(createdRequest);
        return createdDto;
    }

    // ---------------- CREATE INTERNAL REQUEST ----------------
    public async Task<ErrorOr<GetAllEngineerRequestDto>> CreateInternalRequestAsync(CreateInternalRequestDto dto)
    {
        var roles = CurrentUser.Roles;
        var isOfficeEngineer = roles.Any(r => r.Equals(RoleNames.Officeengineer, StringComparison.OrdinalIgnoreCase));
        var isTeamLead = roles.Any(r => r.Equals(RoleNames.Teamleadengineer, StringComparison.OrdinalIgnoreCase));
        var isSuperAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase));
        if (!isOfficeEngineer && !isTeamLead && !isSuperAdmin)
            return Error.Forbidden("Auth.Forbidden", "Only Office Engineers or Team Leads can create Internal Requests.");

        var engineer = await _db.Engineers
            .Include(e => e.Department)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        if (engineer is null)
            return Error.NotFound("Engineer.NotFound", _localizer[SharedResourcesKeys.NotFound]);

        // Resolve the requester's branch (via their primary department)
        var requesterBranchId = engineer.Department?.BranchId;
        if (requesterBranchId is null)
            return Error.Validation("Engineer.NoBranch", "Your account is not assigned to a branch. Contact your administrator.");

        // Validate the selected department belongs to the same branch
        var targetDept = await _db.Departmentes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Id == dto.DepartmentId && !d.IsDeleted);

        if (targetDept is null)
            return Error.NotFound("Department.NotFound", _localizer[SharedResourcesKeys.NotFound]);

        if (!isSuperAdmin && targetDept.BranchId != requesterBranchId)
            return Error.Validation("Department.WrongBranch", "The selected department does not belong to your branch.");

        // Validate the target engineer belongs to that department (via EngineerDepartments or legacy DepartmentId)
        var assignedEngineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == dto.AssignToEngineerId && !e.IsDeleted);

        if (assignedEngineer is null)
            return Error.NotFound("Engineer.NotFound", _localizer[SharedResourcesKeys.NotFound]);

        var engineerInDept = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == dto.AssignToEngineerId && ed.DepartmentId == dto.DepartmentId)
            || assignedEngineer.DepartmentId == dto.DepartmentId;

        if (!engineerInDept)
            return Error.Validation("Engineer.NotInDepartment", "The selected engineer does not belong to the chosen department.");

        var firstStatus = await _db.Statuses
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.orderNumber == 1);

        var request = new EngineerRequest
        {
            RequestType    = "InternalRequest",
            EngineerId     = engineer.Id,
            assignToId     = dto.AssignToEngineerId,
            DepartmentId   = dto.DepartmentId,
            PriorityId     = dto.PriorityId == Guid.Empty ? null : dto.PriorityId,
            RequestTitle   = dto.RequestTitle,
            Descreption    = dto.Descreption,
            StatusId       = firstStatus?.Id ?? Guid.Empty,
            EngineerRequestNotes      = new List<EngineerRequestNotes>(),
            EngineerRequestActivites  = new List<EngineerRequestActivite>(),
            EngineerRequestAttachments = new List<EngineerRequestAttachment>()
        };

        // Seed notes field with the Notes text if provided
        if (!string.IsNullOrWhiteSpace(dto.Notes))
        {
            request.EngineerRequestNotes.Add(new EngineerRequestNotes
            {
                EngineerId = engineer.Id,
                note       = dto.Notes
            });
        }

        // Upload attachments
        if (dto.Attachments != null && dto.Attachments.Any())
        {
            var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
            request.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
            {
                Key       = f.Key,
                FileName  = f.FileName,
                Extension = f.Extension,
                FileSize  = f.FileSize,
                Url       = f.Url
            }).ToList() ?? new List<EngineerRequestAttachment>();
        }

        // Seed activity: Created + immediately Assigned
        request.EngineerRequestActivites.Add(new EngineerRequestActivite
        {
            EngineerId = engineer.Id,
            StatusId   = request.StatusId,
            ActionType = EngineerRequestActionType.Created.ToString()
        });
        request.EngineerRequestActivites.Add(new EngineerRequestActivite
        {
            EngineerId = engineer.Id,
            StatusId   = request.StatusId,
            ActionType = EngineerRequestActionType.Assigned.ToString()
        });

        await _db.EngineerRequests.AddAsync(request);
        await _db.SaveChangesAsync();

        // Notify the assigned engineer
        await _notificationService.SendNotificationToUserAsync(
            assignedEngineer.ApplicationUserId,
            _localizer[SharedResourcesKeys.NotificationAssignedTitle],
            _localizer[SharedResourcesKeys.NotificationAssignedBody],
            request.Id,
            request.DepartmentId);

        // Reload with navigation properties
        var created = await _db.EngineerRequests
            .Include(r => r.Department)
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.Engineer)
            .Include(r => r.assignTo)
            .Include(r => r.EngineerRequestNotes).ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestNotes).ThenInclude(n => n.Engineer)
            .Include(r => r.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestActivites)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        var dto2 = _mapper.Map<GetAllEngineerRequestDto>(created);
        dto2.RequestType = "InternalRequest";
        return dto2;
    }

    // ---------------- UPDATE ----------------
    public async Task<ErrorOr<GetAllEngineerRequestDto>> UpdateEngineerRequestAsync(UpdateEngineerRequestDto dto)
    {
        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        var request = await _db.EngineerRequests
            .Include(r => r.EngineerRequestNotes)
            .Include(r => r.SpecialFieldValues)
            .Include(r => r.SpecialFieldItems)
            .Include(r => r.SpecialFieldListItems)
            .Include(r => r.Status)
            .FirstOrDefaultAsync(r => r.Id == dto.Id);
        if (request is null)
            return Error.NotFound("Request.NotFound", _localizer[SharedResourcesKeys.RequestNotFound]);

        // Rejected requests are final — cannot be edited
        if (request.Status != null && StatusMatchesKeywords(request.Status, RejectedKeywords))
            return Error.Forbidden("Request.Rejected", _localizer[SharedResourcesKeys.RequestRejectedCannotEdit]);

        bool isPendingInfo = request.Status != null && StatusMatchesKeywords(request.Status, PendingInfoKeywords);

        // Allow update only when: status is Missing Information, OR request not yet assigned
        if (!isPendingInfo && request.assignToId != null && request.assignToId != Guid.Empty)
            return Error.Forbidden("Request.AlreadyActioned", _localizer[SharedResourcesKeys.RequestAlreadyActioned]);

        // When in Missing Information status, at least one note is mandatory (attachment is optional)
        if (isPendingInfo)
        {
            bool hasNote = dto.EngineerRequestNotes != null && dto.EngineerRequestNotes.Any();
            if (!hasNote)
                return Error.Validation("Request.MissingNote", _localizer[SharedResourcesKeys.MissingInfoRequiresNoteAndAttachment]);
        }

        // Update fields
        // ProjectId is nullable and, when the multipart form omits it entirely, ASP.NET Core
        // model binding leaves it at its default (null) rather than leaving it "untouched".
        // That made "omitted" indistinguishable from "explicitly clear the project", so any
        // update that didn't resend ProjectId wiped it. Only overwrite when a real, non-empty
        // value was actually supplied; otherwise leave the existing value as-is.
        if (dto.ProjectId.HasValue && dto.ProjectId.Value != Guid.Empty)
        {
            request.ProjectId = dto.ProjectId;
        }
        request.DepartmentId = dto.DepartmentId == Guid.Empty ? request.DepartmentId : dto.DepartmentId;
        request.PriorityId = dto.PriorityId == Guid.Empty ? request.PriorityId : dto.PriorityId;
        request.RequestTitle = dto.RequestTitle ?? request.RequestTitle;
        request.Descreption = dto.Descreption;

        // Handle notes
        if (dto.EngineerRequestNotes != null)
        {
            // Remove notes not in DTO
            var dtoNoteIds = dto.EngineerRequestNotes
                .Where(n => n.Id.HasValue)
                .Select(n => n.Id.Value)
                .ToList();

            var notesToRemove = request.EngineerRequestNotes
                .Where(n => !dtoNoteIds.Contains(n.Id))
                .ToList();

            _db.EngineerRequestNotes.RemoveRange(notesToRemove);

            // Update or add notes
            foreach (var noteDto in dto.EngineerRequestNotes)
            {
                if (noteDto.Id.HasValue)
                {
                    // Update existing note
                    var existingNote = request.EngineerRequestNotes.FirstOrDefault(n => n.Id == noteDto.Id);
                    if (existingNote != null)
                    {
                        existingNote.note = noteDto.note;
                        // If there are new attachments for existing note, upload and add
                        if (noteDto.Attachments != null && noteDto.Attachments.Any())
                        {
                            var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                            if (uploaded != null && uploaded.Any())
                            {
                                existingNote.EngineerRequestAttachments ??= new List<EngineerRequestAttachment>();
                                existingNote.EngineerRequestAttachments = existingNote.EngineerRequestAttachments.Concat(uploaded.Select(f => new EngineerRequestAttachment
                                {
                                    Key = f.Key,
                                    FileName = f.FileName,
                                    Extension = f.Extension,
                                    FileSize = f.FileSize,
                                    Url = f.Url
                                })).ToList();
                            }
                        }
                    }
                }
                else
                {
                    // Add new note
                    var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                    newNote.EngineerRequestId = request.Id;
                    newNote.EngineerId = engineer?.Id;
                    // Handle attachments for new note
                    if (noteDto.Attachments != null && noteDto.Attachments.Any())
                    {
                        var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                        newNote.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
                        {
                            Key = f.Key,
                            FileName = f.FileName,
                            Extension = f.Extension,
                            FileSize = f.FileSize,
                            Url = f.Url
                        }).ToList();
                    }

                    await _db.EngineerRequestNotes.AddAsync(newNote);
                }
            }
        }

        // Handle request-level new attachments
        if (dto.Attachments != null && dto.Attachments.Any())
        {
            var uploaded = await _storageService.UploadFiles(dto.Attachments.ToList());
            if (uploaded != null && uploaded.Any())
            {
                request.EngineerRequestAttachments ??= new List<EngineerRequestAttachment>();
                request.EngineerRequestAttachments = request.EngineerRequestAttachments.Concat(uploaded.Select(f => new EngineerRequestAttachment
                {
                    Key = f.Key,
                    FileName = f.FileName,
                    Extension = f.Extension,
                    FileSize = f.FileSize,
                    Url = f.Url,
                    EngineerRequestId = request.Id
                })).ToList();
            }
        }

        // Handle special field values
        if (dto.SpecialFieldValues != null)
        {
            // Validate ConstructionItem special fields before persisting
            var updateValidationError = await ValidateConstructionItemFieldsAsync(dto.SpecialFieldValues);
            if (updateValidationError.HasValue) return updateValidationError.Value;

            // Remove existing special field values
            if (request.SpecialFieldValues != null && request.SpecialFieldValues.Any())
            {
                _db.EngineerRequestSpecialFieldValues.RemoveRange(request.SpecialFieldValues);
            }

            // Add new special field values
            foreach (var sfv in dto.SpecialFieldValues)
            {
                var specialFieldValue = new EngineerRequestSpecialFieldValue
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = sfv.DepartmentSpecialFieldId,
                    value = sfv.value
                };
                await _db.EngineerRequestSpecialFieldValues.AddAsync(specialFieldValue);
            }
        }

        // Handle special field items (ConstructionItem + Quantity list for Procurement)
        if (dto.SpecialFieldItems != null)
        {
            // Remove existing items
            if (request.SpecialFieldItems != null && request.SpecialFieldItems.Any())
            {
                _db.EngineerRequestSpecialFieldItems.RemoveRange(request.SpecialFieldItems);
            }

            if (dto.SpecialFieldItems.Any())
            {
                // Validate all ConstructionItem IDs exist
                var itemIds = dto.SpecialFieldItems.Select(i => i.ConstructionItemId).Distinct().ToList();
                var existingItemIds = await _db.ConstructionItems
                    .Where(ci => itemIds.Contains(ci.Id))
                    .Select(ci => ci.Id)
                    .ToListAsync();
                var missingId = itemIds.Except(existingItemIds).FirstOrDefault();
                if (missingId != Guid.Empty)
                    return Error.NotFound("ConstructionItem.NotFound", _localizer[SharedResourcesKeys.ConstructionItemNotFound]);

                foreach (var item in dto.SpecialFieldItems)
                {
                    await _db.EngineerRequestSpecialFieldItems.AddAsync(new EngineerRequestSpecialFieldItem
                    {
                        EngineerRequestId = request.Id,
                        DepartmentSpecialFieldId = item.DepartmentSpecialFieldId,
                        ConstructionItemId = item.ConstructionItemId,
                        Quantity = item.Quantity
                    });
                }
            }
        }

        // Handle special field list items (repeatable row values for list-flagged fields)
        if (dto.SpecialFieldListItems != null)
        {
            if (dto.SpecialFieldListItems.Any())
            {
                var sfliFieldIds = dto.SpecialFieldListItems.Select(i => i.DepartmentSpecialFieldId).Distinct().ToList();
                var existingSfliFields = await _db.DepartmentSpecialFields
                    .Where(dsf => sfliFieldIds.Contains(dsf.Id))
                    .Select(dsf => new { dsf.Id, dsf.ListGroupKey })
                    .ToListAsync();
                var missingSfliFieldId = sfliFieldIds.Except(existingSfliFields.Select(f => f.Id)).FirstOrDefault();
                if (missingSfliFieldId != Guid.Empty)
                    return Error.NotFound("DepartmentSpecialField.NotFound", _localizer[SharedResourcesKeys.NotFound]);
                if (existingSfliFields.Any(f => string.IsNullOrWhiteSpace(f.ListGroupKey)))
                    return Error.Validation("SpecialField.NotList", "This field is not configured as a repeatable list.");
            }

            // Remove existing list items
            if (request.SpecialFieldListItems != null && request.SpecialFieldListItems.Any())
            {
                _db.EngineerRequestSpecialFieldListItems.RemoveRange(request.SpecialFieldListItems);
            }

            // Add new list items
            foreach (var item in dto.SpecialFieldListItems)
            {
                await _db.EngineerRequestSpecialFieldListItems.AddAsync(new EngineerRequestSpecialFieldListItem
                {
                    EngineerRequestId = request.Id,
                    DepartmentSpecialFieldId = item.DepartmentSpecialFieldId,
                    RowIndex = item.RowIndex,
                    value = item.value
                });
            }
        }

        await _db.SaveChangesAsync();

        // If request was in Missing Information, auto-reset status back to New
        if (isPendingInfo)
        {
            var allStatuses = await _db.Statuses
                .AsNoTracking()
                .OrderBy(s => s.orderNumber)
                .ToListAsync();

            var targetStatus = allStatuses.FirstOrDefault(s => StatusMatchesKeywords(s, NewPendingKeywords))
                            ?? allStatuses.OrderBy(s => s.orderNumber).FirstOrDefault();

            if (targetStatus != null && targetStatus.Id != request.StatusId)
            {
                request.StatusId = targetStatus.Id;
                var resetActivity = new EngineerRequestActivite
                {
                    EngineerRequestId = request.Id,
                    EngineerId = engineer?.Id,
                    StatusId = targetStatus.Id,
                    ActionType = EngineerRequestActionType.StatusChangedAuto.ToString()
                };
                await _db.EngineerRequestActivites.AddAsync(resetActivity);
                await _db.SaveChangesAsync();
            }
        }

        // Reload with navigation properties (including attachments)
        var updatedRequest = await _db.EngineerRequests
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Priority)
            .Include(r => r.Engineer)
                .ThenInclude(e => e.Department)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.Engineer)
            .Include(r => r.EngineerRequestAttachments)
            .Include(r => r.SpecialFieldValues)
                .ThenInclude(v => v.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .Include(r => r.SpecialFieldItems)
                .ThenInclude(i => i.ConstructionItem)
                    .ThenInclude(c => c.Units)
            .Include(r => r.SpecialFieldListItems)
                .ThenInclude(i => i.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == request.Id);

        var updatedDto = _mapper.Map<GetAllEngineerRequestDto>(updatedRequest);
        return updatedDto;
    }

    // ---------------- DELETE ----------------
    public async Task<GenericResponse> DeleteEngineerRequestAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests.FindAsync(requestId);
        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);

        // Check if action has been taken
        if (request.assignToId != Guid.Empty)
        {
            // Request has been actioned, cannot delete
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestAlreadyActioned]);
        }

        _db.EngineerRequests.Remove(request);
        await _db.SaveChangesAsync();
        return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.RequestDeleteSuccess]);
    }

    // ---------------- GET ALL BY DEPARTMENT (FOR MANAGERS) ----------------
    public async Task<PaginatedList<GetAllEngineerRequestDto>> GetAllEngineerRequestsByDepartmentAsync(
        Guid departmentId,
        BaseFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r=>r.Status)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.Engineer)
                .Include(r => r.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.SpecialFieldValues)
                    .ThenInclude(v => v.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .Include(r => r.SpecialFieldItems)
                    .ThenInclude(i => i.ConstructionItem)
                        .ThenInclude(c => c.Units)
                .Include(r => r.SpecialFieldListItems)
                    .ThenInclude(i => i.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .Where(r => r.DepartmentId == departmentId)
                .AsSplitQuery()
                .AsNoTracking();

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var requestDtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);

            return new PaginatedList<GetAllEngineerRequestDto>(
                requestDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    // ---------------- FILTER ENGINEER REQUESTS ----------------
    public async Task<PaginatedList<GetAllEngineerRequestDto>> FilterEngineerRequestsAsync(
        EngineerRequestFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _db.EngineerRequests
                .Include(r => r.Project)
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.EngineerRequestAttachments)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.Department)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.ApplicationUser)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.SpecialFieldValues)
                    .ThenInclude(v => v.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .Include(r => r.SpecialFieldItems)
                    .ThenInclude(i => i.ConstructionItem)
                        .ThenInclude(c => c.Units)
                .Include(r => r.SpecialFieldListItems)
                    .ThenInclude(i => i.DepartmentSpecialField)
                        .ThenInclude(psf => psf.SpecialField)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            {
                query = query.Where(r => r.DepartmentId == filter.DepartmentId.Value);
            }

            if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
            {
                query = query.Where(r => r.StatusId == filter.StatusId.Value);
            }

            if (filter.EngineerId.HasValue && filter.EngineerId.Value != Guid.Empty)
            {
                query = query.Where(r => r.assignToId == filter.EngineerId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(r => r.startDate.HasValue && r.startDate.Value >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(r => r.endDate.HasValue && r.endDate.Value <= filter.ToDate.Value);
            }

            if (string.IsNullOrWhiteSpace(filter.Sort))
            {
                query = query.OrderByDescending(r => r.CreatedDate);
            }
            else
            {
                query = query.OrderByDynamic(filter.Sort, filter.Descending);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
            {
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var requestDtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);

            return new PaginatedList<GetAllEngineerRequestDto>(
                requestDtos,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    // ---------------- GET ALL INTERNAL REQUESTS ----------------
    public async Task<PaginatedList<GetAllEngineerRequestDto>> GetAllInternalRequestsAsync(
        InternalRequestFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // ── Role resolution ──────────────────────────────────────────────────
            var roles   = CurrentUser.Roles;
            var isAdmin = roles.Any(r =>
                r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
                r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

            Engineer? currentEngineer = null;
            List<Guid> teamLeadDeptIds = new();

            if (!isAdmin)
            {
                currentEngineer = await _db.Engineers
                    .AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId!), cancellationToken);

                if (currentEngineer is null)
                    return new PaginatedList<GetAllEngineerRequestDto>(
                        new List<GetAllEngineerRequestDto>(), 0, filter.PageIndex, filter.PageSize);

                // Departments where the current engineer is team lead (via EngineerDepartments)
                teamLeadDeptIds = await (from ed in _db.EngineerDepartments
                                         join role in _db.Roles on ed.RoleId equals role.Id
                                         where ed.EngineerId == currentEngineer.Id
                                               && role.Name == RoleNames.Teamleadengineer
                                         select ed.DepartmentId)
                                        .ToListAsync(cancellationToken);

                // Fallback: legacy DepartmentId + global role assignment
                if (!teamLeadDeptIds.Any() && currentEngineer.DepartmentId.HasValue)
                {
                    var isTeamLeadViaRoles = await (from ur in _db.UserRoles
                                                    join role in _db.Roles on ur.RoleId equals role.Id
                                                    where ur.UserId == currentEngineer.ApplicationUserId
                                                          && role.Name == RoleNames.Teamleadengineer
                                                    select role.Id).AnyAsync(cancellationToken);
                    if (isTeamLeadViaRoles)
                        teamLeadDeptIds.Add(currentEngineer.DepartmentId.Value);
                }
            }

            // ── Base query ───────────────────────────────────────────────────────
            var query = _db.EngineerRequests
                .Include(r => r.Department)
                .Include(r => r.Priority)
                .Include(r => r.Status)
                .Include(r => r.Engineer)
                    .ThenInclude(e => e.Department)
                .Include(r => r.assignTo)
                    .ThenInclude(e => e.Department)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.EngineerRequestAttachments)
                .Include(r => r.EngineerRequestNotes)
                    .ThenInclude(n => n.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Engineer)
                .Include(r => r.EngineerRequestActivites)
                    .ThenInclude(a => a.Status)
                .Include(r => r.EngineerRequestAttachments)
                .AsSplitQuery()
                .AsNoTracking()
                .Where(r => !r.IsDeleted && r.RequestType == "InternalRequest");

            // ── Role-based visibility ────────────────────────────────────────────
            if (!isAdmin)
            {
                var engineerId = currentEngineer!.Id;

                if (teamLeadDeptIds.Any())
                {
                    // Team lead: all requests in their departments
                    query = query.Where(r =>
                        r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value));
                }
                else
                {
                    // Office engineer (or any non-admin non-lead): own requests + assigned to them
                    query = query.Where(r =>
                        r.EngineerId == engineerId || r.assignToId == engineerId);
                }
            }

            // ── Explicit filters (applied on top of visibility) ─────────────────
            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
                query = query.Where(r => r.DepartmentId == filter.DepartmentId.Value);

            if (filter.BranchId.HasValue && filter.BranchId.Value != Guid.Empty)
                query = query.Where(r => r.Department != null && r.Department.BranchId == filter.BranchId.Value);

            if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
                query = query.Where(r => r.StatusId == filter.StatusId.Value);

            if (filter.PriorityId.HasValue && filter.PriorityId.Value != Guid.Empty)
                query = query.Where(r => r.PriorityId == filter.PriorityId.Value);

            if (filter.RequestedById.HasValue && filter.RequestedById.Value != Guid.Empty)
                query = query.Where(r => r.EngineerId == filter.RequestedById.Value);

            if (filter.AssignedToId.HasValue && filter.AssignedToId.Value != Guid.Empty)
                query = query.Where(r => r.assignToId == filter.AssignedToId.Value);

            if (filter.FromDate.HasValue)
                query = query.Where(r => r.CreatedDate >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(r => r.CreatedDate <= filter.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
                query = query.Where(r =>
                    (r.RequestTitle != null && r.RequestTitle.Contains(filter.Search)) ||
                    (r.Descreption  != null && r.Descreption.Contains(filter.Search)));

            query = string.IsNullOrWhiteSpace(filter.Sort)
                ? query.OrderByDescending(r => r.CreatedDate)
                : query.OrderByDynamic(filter.Sort, filter.Descending);

            var totalCount = await query.CountAsync(cancellationToken);

            if (totalCount == 0)
                return new PaginatedList<GetAllEngineerRequestDto>(
                    new List<GetAllEngineerRequestDto>(), 0, filter.PageIndex, filter.PageSize);

            var requests = await query
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync(cancellationToken);

            var dtos = _mapper.Map<List<GetAllEngineerRequestDto>>(requests);
            dtos.ForEach(d => d.RequestType = "InternalRequest");

            return new PaginatedList<GetAllEngineerRequestDto>(
                dtos, totalCount, filter.PageIndex, filter.PageSize);
        }
        catch (Exception)
        {
            return new PaginatedList<GetAllEngineerRequestDto>(
                new List<GetAllEngineerRequestDto>(), 0, filter.PageIndex, filter.PageSize);
        }
    }

    // ---------------- GET BY ID ----------------
    public async Task<GetAllEngineerRequestDto> GetEngineerRequestByIdAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Priority)
            .Include(r => r.Status)
            .Include(r => r.Engineer)
                .ThenInclude(e => e.Department)
            .Include(r => r.assignTo)
                .ThenInclude(e => e.Department)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.Engineer)
            .Include(r => r.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Engineer)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Status)
            .Include(r => r.SpecialFieldValues)
                .ThenInclude(v => v.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .Include(r => r.SpecialFieldItems)
                .ThenInclude(i => i.ConstructionItem)
                    .ThenInclude(c => c.Units)
            .Include(r => r.SpecialFieldListItems)
                .ThenInclude(i => i.DepartmentSpecialField)
                    .ThenInclude(psf => psf.SpecialField)
            .Include(r => r.PurchaseReceipts)
                .ThenInclude(rc => rc.ReceivedBy)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null) return null!;

        var dto = _mapper.Map<GetAllEngineerRequestDto>(request);

        // Manually populate goods receipt fields (not covered by Mapster auto-map)
        dto.Receipts = request.PurchaseReceipts?
            .OrderByDescending(rc => rc.ReceiptDate)
            .Select(MapReceiptToDto)
            .ToList() ?? new();

        return dto;
    }

    // ---------------- CHECK IF ENGINEER IS MANAGER ----------------
    public async Task<bool> IsEngineerManagerOfDepartmentAsync(Guid engineerId, Guid departmentId)
    {
        var teamleadRoleName = RoleNames.Teamleadengineer;

        // Check via EngineerDepartments (multi-department role assignment)
        var isTeamLeadViaDeptRole = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == engineerId
                         && ed.DepartmentId == departmentId
                         && ed.Role != null && ed.Role.Name == teamleadRoleName);

        if (isTeamLeadViaDeptRole) return true;

        // Fallback: check via legacy DepartmentId + global UserRoles
        var isTeamLead = await (from eng in _db.Engineers
                                join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                join role in _db.Roles on userRole.RoleId equals role.Id
                                where eng.Id == engineerId && eng.DepartmentId == departmentId && role.Name == teamleadRoleName
                                select eng.Id)
                    .AnyAsync();

        return isTeamLead;
    }

    // ---------------- TAKE ACTION ON REQUEST ----------------
    public async Task<GenericResponse> TakeActionOnRequestAsync(Guid requestId, Guid currentUserId, TakeActionRequestDto actionDto)
    {

        var request = await _db.EngineerRequests
            .Include(r => r.EngineerRequestNotes)
            .Include(x=>x.Engineer).ThenInclude(x=>x.ApplicationUser)
            .Include(r => r.assignTo)
            .Include(r => r.Status)
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == requestId);


        var currentEngineerId = await _db.Engineers
            .Where(e => e.ApplicationUserId == currentUserId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);

        // Rejected requests are final — no further actions allowed
        if (request.Status != null && StatusMatchesKeywords(request.Status, RejectedKeywords))
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestRejectedCannotAction]);
      
        // Get department manager
        var departmentId = request.DepartmentId;
        if (!departmentId.HasValue)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNoDepartment]);

        // Check if department has a team lead
        bool departmentHasTeamLead = await DepartmentHasTeamLeadAsync(departmentId.Value);

        // Check if current user is a team lead for this request's department (via EngineerDepartments or legacy)
        var isManager = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == currentEngineerId
                         && ed.DepartmentId == departmentId.Value
                         && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer);
        if (!isManager)
        {
            isManager = await (from eng in _db.Engineers
                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                               join role in _db.Roles on userRole.RoleId equals role.Id
                               where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                               select eng.Id)
                       .AnyAsync();
        }

        var isAssigned = request.assignToId.HasValue && request.assignToId.Value == currentEngineerId;

        // If assignToId is set
        if (request.assignToId.HasValue && request.assignToId.Value != Guid.Empty)
        {
            if (departmentHasTeamLead)
            {
                // If department has a team lead, only assigned user or team lead can take action
                if (!isManager && !isAssigned)
                    return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
            }
            else
            {
                // No team lead: only assigned user can take action
                if (!isAssigned)
                    return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
            }
        }
        else if (departmentHasTeamLead)
        {
            // If department has a team lead, only team lead can take action
            if (!isManager)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }
        else
        {
            // If no team lead and no assignment, allow any engineer in the department to take action
            var isEngineerInDepartment = await _db.EngineerDepartments.AnyAsync(ed => ed.EngineerId == currentEngineerId && ed.DepartmentId == departmentId.Value)
                || await _db.Engineers.AnyAsync(e => e.ApplicationUserId == currentUserId && e.DepartmentId == departmentId.Value);
            if (!isEngineerInDepartment)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }

        // Track previous status and assignment for activity
        var previousStatusId = request.StatusId;
        var previousAssignedId = request.assignToId;
        bool assignmentChanged = false;

        // If manager, allow assignment
        if (isManager && actionDto.assignToId.HasValue)
        {
            request.assignToId = actionDto.assignToId.Value;
            
            // Create activity for assignment
            if (previousAssignedId != actionDto.assignToId.Value)
            {
                assignmentChanged = true;
                var assignActivity = new EngineerRequestActivite
                {
                    EngineerRequestId = request.Id,
                    EngineerId = currentEngineerId,
                    StatusId = request.StatusId,
                    ActionType = EngineerRequestActionType.Assigned.ToString()
                };
                await _db.EngineerRequestActivites.AddAsync(assignActivity);
            }
        }
        // If assigned engineer, do not allow assignment change
        else if (isAssigned && actionDto.assignToId.HasValue && actionDto.assignToId.Value != currentEngineerId)
        {
            // Assigned engineer cannot reassign
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);
        }
        // Department without team lead: auto-assign the request to the engineer taking action
        else if (!departmentHasTeamLead && !isAssigned
            && (!request.assignToId.HasValue || request.assignToId.Value == Guid.Empty))
        {
            request.assignToId = currentEngineerId;
            assignmentChanged = true;
            var assignActivity = new EngineerRequestActivite
            {
                EngineerRequestId = request.Id,
                EngineerId = currentEngineerId,
                StatusId = request.StatusId,
                ActionType = EngineerRequestActionType.Assigned.ToString()
            };
            await _db.EngineerRequestActivites.AddAsync(assignActivity);
        }

        // Update status, note, and note date
        if (actionDto.statusId.HasValue && actionDto.statusId.Value != previousStatusId)
        {
            request.StatusId = actionDto.statusId.Value;
            
            // Create activity for status change
            var statusActivity = new EngineerRequestActivite
            {
                EngineerRequestId = request.Id,
                EngineerId = currentEngineerId,
                StatusId = actionDto.statusId.Value,
                ActionType = EngineerRequestActionType.StatusChanged.ToString()
            };
            await _db.EngineerRequestActivites.AddAsync(statusActivity);
        }

        //if (isAssigned && request.timeDuration != actionDto.timeDuration.Value)
        //{
        //    return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.TimeDurationMismatch]);
        //}

        if (actionDto.timeDuration.HasValue && actionDto.timeDuration != 0)
        {
            request.timeDuration = actionDto.timeDuration.Value;

            if (actionDto.startDate.HasValue)
                request.startDate = actionDto.startDate.Value;

            // Block endDate change if delivery date has already been confirmed
            if (request.IsDeliveryDateConfirmed && actionDto.endDate != request.endDate)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.DeliveryDateAlreadyConfirmed]);

            request.endDate = actionDto.endDate;
        }                        

        if (actionDto.EngineerRequestNotes != null && actionDto.EngineerRequestNotes.Any())
        {
            foreach (var noteDto in actionDto.EngineerRequestNotes)
            {
                var newNote = _mapper.Map<EngineerRequestNotes>(noteDto);
                newNote.EngineerRequestId = request.Id;
                newNote.EngineerId = currentEngineerId;

                // Handle attachments for the new note
                if (noteDto.Attachments != null && noteDto.Attachments.Any())
                {
                    var uploaded = await _storageService.UploadFiles(noteDto.Attachments.ToList());
                    newNote.EngineerRequestAttachments = uploaded?.Select(f => new EngineerRequestAttachment
                    {
                        Key = f.Key,
                        FileName = f.FileName,
                        Extension = f.Extension,
                        FileSize = f.FileSize,
                        Url = f.Url
                    }).ToList();
                }

                await _db.EngineerRequestNotes.AddAsync(newNote);
            }
        }

        // Save received quantities — only when the request is awaiting site engineer receipt confirmation
        if (request.NeedsReceiptConfirmation && actionDto.SpecialFieldItems != null && actionDto.SpecialFieldItems.Any())
        {
            // Load all of the request's special field item rows (not just the ones filtered by submitted
            // ids) so we can match by row Id or, when unambiguous, by ConstructionItemId — see
            // ApplySpecialFieldItemReceivedQuantities for the matching rules.
            var dbItems = await _db.EngineerRequestSpecialFieldItems
                .Where(i => i.EngineerRequestId == requestId)
                .ToListAsync();

            var unresolved = ApplySpecialFieldItemReceivedQuantities(dbItems, actionDto.SpecialFieldItems);
            if (unresolved.Count > 0)
            {
                return GenericResponse.FailureResult(
                    $"Item(s) {string.Join(", ", unresolved)} were not found on this request.");
            }
        }

        await _db.SaveChangesAsync();

        // Send notifications
        if (isManager && request.EngineerId.HasValue && request.Engineer?.ApplicationUserId != Guid.Empty)
        {
            // Notify the request creator that team lead took action
            await _notificationService.SendNotificationToUserAsync(
                request.Engineer.ApplicationUserId,
                _localizer[SharedResourcesKeys.NotificationRequestUpdatedTitle],
                _localizer[SharedResourcesKeys.NotificationRequestUpdatedBody],
                request.Id);
        }

        // Only notify when assignment actually changed (new assignment or reassignment)
        if (assignmentChanged && request.assignToId.HasValue)
        {
            var assignEngineer = await _db.Engineers
            .FirstOrDefaultAsync(r => r.Id == request.assignToId);
            // Notify the engineer who was assigned
            await _notificationService.SendNotificationToUserAsync(
                assignEngineer.ApplicationUserId,
                _localizer[SharedResourcesKeys.NotificationAssignedTitle],
                _localizer[SharedResourcesKeys.NotificationAssignedBody],
                request.Id);
        }

        // If the current user is the assigned engineer and not the team lead
        if (isAssigned && request.EngineerId.HasValue)
        {
            var creator = await _db.Engineers
                .Where(e => e.Id == request.EngineerId.Value)
                .Select(e => e.ApplicationUserId)
                .FirstOrDefaultAsync();

            // Notify the request creator that assigned engineer took action
            await _notificationService.SendNotificationToUserAsync(
                creator,
                _localizer[SharedResourcesKeys.NotificationAssignedUpdateTitle],
                _localizer[SharedResourcesKeys.NotificationAssignedUpdateBody],
                request.Id);
        }

        // Notify request creator if status was changed to Missing Information
        if (actionDto.statusId.HasValue)
        {
            var newStatus = await _db.Statuses.FindAsync(request.StatusId);
            if (newStatus != null && StatusMatchesKeywords(newStatus, PendingInfoKeywords)
                && request.EngineerId.HasValue)
            {
                var creatorAppUserId = request.Engineer?.ApplicationUserId ?? Guid.Empty;
                if (creatorAppUserId != Guid.Empty)
                {
                    await _notificationService.SendNotificationToUserAsync(
                        creatorAppUserId,
                        _localizer[SharedResourcesKeys.NotificationMissingInfoTitle],
                        _localizer[SharedResourcesKeys.NotificationMissingInfoBody],
                        request.Id);
                }
            }

            // When office engineer sets status to completed/confirmed → require site engineer receipt confirmation
            // Only applies to departments that have RequiresGoodsReceipt enabled (e.g. Procurement)
            if (newStatus != null && StatusMatchesKeywords(newStatus, CompletedKeywords)
                && request.EngineerId.HasValue
                && request.Department != null && request.Department.RequiresGoodsReceipt)
            {
                request.NeedsReceiptConfirmation = true;
                await _db.SaveChangesAsync();

                var creatorAppUserId = request.Engineer?.ApplicationUserId ?? Guid.Empty;
                if (creatorAppUserId != Guid.Empty)
                {
                    await _notificationService.SendNotificationToUserAsync(
                        creatorAppUserId,
                        _localizer[SharedResourcesKeys.NotificationReceiptRequiredTitle],
                        _localizer[SharedResourcesKeys.NotificationReceiptRequiredBody],
                        request.Id);
                }
            }
        }

        return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.ActionTakenSuccess]);
    }

    // ---------------- REASSIGN REQUEST ----------------
    public async Task<GenericResponse> ReassignEngineerRequestAsync(Guid requestId, Guid currentUserId, ReassignEngineerRequestDto dto)
    {
        if (dto == null || dto.assignToId == Guid.Empty)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);

        var request = await _db.EngineerRequests
            .Include(r => r.Engineer)
            .Include(r => r.assignTo)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNotFound]);

        if (!request.assignToId.HasValue || request.assignToId.Value == Guid.Empty)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);

        var departmentId = request.DepartmentId;
        if (!departmentId.HasValue)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.RequestNoDepartment]);

        var departmentHasTeamLead = await DepartmentHasTeamLeadAsync(departmentId.Value);

        var currentEngineerId = await _db.Engineers
            .Where(e => e.ApplicationUserId == currentUserId)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        // Check if current user is a team lead for this request's department (via EngineerDepartments or legacy)
        var isManager = await _db.EngineerDepartments
            .AnyAsync(ed => ed.EngineerId == currentEngineerId
                         && ed.DepartmentId == departmentId.Value
                         && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer);
        if (!isManager)
        {
            isManager = await (from eng in _db.Engineers
                               join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                               join role in _db.Roles on userRole.RoleId equals role.Id
                               where eng.DepartmentId == departmentId.Value && role.Name == RoleNames.Teamleadengineer && eng.ApplicationUserId == currentUserId
                               select eng.Id)
                       .AnyAsync();
        }

        var isAssigned = request.assignToId.Value == currentEngineerId;

        if (departmentHasTeamLead)
        {
            if (!isManager)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }
        else
        {
            if (!isAssigned)
                return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.Unauthorized]);
        }

        if (request.assignToId.Value == dto.assignToId)
            return GenericResponse.FailureResult(_localizer[SharedResourcesKeys.CannotReassign]);

        request.assignToId = dto.assignToId;

        var reassignActivity = new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = currentEngineerId,
            StatusId = request.StatusId,
            ActionType = EngineerRequestActionType.Reassigned.ToString()
        };
        await _db.EngineerRequestActivites.AddAsync(reassignActivity);

        await _db.SaveChangesAsync();

        var assignEngineer = await _db.Engineers
            .FirstOrDefaultAsync(r => r.Id == request.assignToId);

        if (assignEngineer?.ApplicationUserId != null && assignEngineer.ApplicationUserId != Guid.Empty)
        {
            await _notificationService.SendNotificationToUserAsync(
                assignEngineer.ApplicationUserId,
                _localizer[SharedResourcesKeys.NotificationReassignedTitle],
                _localizer[SharedResourcesKeys.NotificationReassignedBody],
                request.Id);
        }

        return GenericResponse.SuccessResult(_localizer[SharedResourcesKeys.ActionTakenSuccess]);
    }



    public async Task<PaginatedList<GetUnifiedRequestDto>> GetCreatedRequestOrapplaied(EngineerRequestParticipationFilterDto filter, CancellationToken cancellationToken = default)
    {
        try
        {
            // Step 1: Get Engineer Requests using ORIGINAL LOGIC with admin/team lead/department filtering
            var engineerRequestsUnified = await GetEngineerRequestsWithOriginalLogicAsync(filter, cancellationToken);

            // Step 2: Get current engineer for other request types
            var engineer = await _db.Engineers
                .Include(x => x.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == Guid.Parse(CurrentUser.UserId), cancellationToken);

            if (engineer == null)
            {
                // Return only engineer requests if user not found (shouldn't happen but be safe)
                return engineerRequestsUnified;
            }

            var allUnifiedRequests = engineerRequestsUnified.Items.ToList();

            // Step 3: Get Transfer Requests (created by engineer)
            var transferRequests = await GetTransferRequestsForUnifiedAsync(engineer, filter, cancellationToken);
            allUnifiedRequests.AddRange(transferRequests);

            // Step 4: Get Labor Attendance Requests (supervised by engineer)
            var laborAttendanceRequests = await GetLaborAttendanceRequestsForUnifiedAsync(engineer, filter, cancellationToken);
            allUnifiedRequests.AddRange(laborAttendanceRequests);

            // Step 5: Get Financial Clearances (created by engineer)
            var financialClearances = await GetFinancialClearancesForUnifiedAsync(engineer, filter, cancellationToken);
            allUnifiedRequests.AddRange(financialClearances);

            // Apply RequestType filter if provided
            if (!string.IsNullOrWhiteSpace(filter.RequestType))
            {
                // InternalRequest is stored as RequestType "InternalRequest" but is also an EngineerRequest
                allUnifiedRequests = allUnifiedRequests
                    .Where(r => string.Equals(r.RequestType, filter.RequestType, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Sort by CreatedDate descending
            var sortedRequests = allUnifiedRequests.OrderByDescending(r => r.CreatedDate).ToList();

            var totalCount = sortedRequests.Count;

            // Apply pagination
            var paginatedRequests = sortedRequests
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            return new PaginatedList<GetUnifiedRequestDto>(
                paginatedRequests,
                totalCount,
                filter.PageIndex,
                filter.PageSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetCreatedRequestOrapplaied failed for user {UserId}", CurrentUser.UserId);
            return new PaginatedList<GetUnifiedRequestDto>(
                new List<GetUnifiedRequestDto>(),
                0,
                filter.PageIndex,
                filter.PageSize);
        }
    }

    // Get EngineerRequests with role-aware filtering for the CreatedOrApplied unified endpoint
    private async Task<PaginatedList<GetUnifiedRequestDto>> GetEngineerRequestsWithOriginalLogicAsync(
        EngineerRequestParticipationFilterDto filter,
        CancellationToken cancellationToken)
    {
        var roles = CurrentUser.Roles;
        var isAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                  || r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        Engineer? engineer = null;
        List<Guid> teamLeadDeptIds = new();
        List<Guid> allEngineerDeptIds = new();

        if (!isAdmin)
        {
            engineer = await _db.Engineers
                .Include(x => x.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == Guid.Parse(CurrentUser.UserId), cancellationToken);

            if (engineer == null)
            {
                return new PaginatedList<GetUnifiedRequestDto>(
                    new List<GetUnifiedRequestDto>(),
                    0,
                    filter.PageIndex,
                    filter.PageSize);
            }

            // Get all departments where engineer is a TeamLead (via EngineerDepartments) � single bulk query
            teamLeadDeptIds = await (from ed in _db.EngineerDepartments
                                     join role in _db.Roles on ed.RoleId equals role.Id
                                     where ed.EngineerId == engineer.Id && role.Name == RoleNames.Teamleadengineer
                                     select ed.DepartmentId)
                                    .ToListAsync(cancellationToken);

            // Fallback: check via legacy DepartmentId + global UserRoles
            if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
            {
                var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                join role in _db.Roles on userRole.RoleId equals role.Id
                                                where userRole.UserId == engineer.ApplicationUserId
                                                      && role.Name == RoleNames.Teamleadengineer
                                                select role.Id).AnyAsync(cancellationToken);
                if (isTeamLeadViaRoles)
                    teamLeadDeptIds.Add(engineer.DepartmentId.Value);
            }

            // Get all departments this engineer belongs to � single query
            allEngineerDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineer.Id)
                .Select(ed => ed.DepartmentId)
                .ToListAsync(cancellationToken);

            // Include the active department if set
            if (engineer.DepartmentId.HasValue && !allEngineerDeptIds.Contains(engineer.DepartmentId.Value))
                allEngineerDeptIds.Add(engineer.DepartmentId.Value);

            // Apply DepartmentId filter to role-based dept lists
            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            {
                var filterDeptId = filter.DepartmentId.Value;
                teamLeadDeptIds = teamLeadDeptIds.Where(d => d == filterDeptId).ToList();
                allEngineerDeptIds = allEngineerDeptIds.Where(d => d == filterDeptId).ToList();
            }
        }

        // Build base query � AsSplitQuery prevents Cartesian explosion from multiple collection includes
        var query = _db.EngineerRequests
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Engineer)
            .Include(r => r.assignTo)
            .Include(r => r.Status)
            .Include(r => r.Priority)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.Engineer)
            .Include(r => r.EngineerRequestNotes)
                .ThenInclude(n => n.EngineerRequestAttachments)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Engineer)
            .Include(r => r.EngineerRequestActivites)
                .ThenInclude(a => a.Status)
            .Include(r => r.SpecialFieldValues)
                .ThenInclude(v => v.DepartmentSpecialField)
                    .ThenInclude(dsf => dsf.SpecialField)
            .Include(r => r.SpecialFieldItems)
                .ThenInclude(i => i.ConstructionItem)
                    .ThenInclude(c => c.Units)
            .Include(r => r.SpecialFieldListItems)
                .ThenInclude(i => i.DepartmentSpecialField)
                    .ThenInclude(dsf => dsf.SpecialField)
            .Include(r => r.EngineerRequestAttachments)
            .Where(r => !r.IsDeleted)
            .AsSplitQuery()
            .AsNoTracking();

        // Optional data filters
        if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            query = query.Where(r => r.ProjectId == filter.ProjectId.Value);

        if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
            query = query.Where(r => r.StatusId == filter.StatusId.Value);

        if (isAdmin)
        {
            // Admin sees all — role-based visibility is unrestricted
        }
        else if (engineer != null)
        {
            bool isTeamLead = teamLeadDeptIds.Any();

            if (isTeamLead)
            {
                // TeamLead: see all requests in their lead-departments + assigned to them + created by them
                query = query.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.assignToId == engineer.Id
                    || r.EngineerId == engineer.Id);
            }
            else if (allEngineerDeptIds.Any())
            {
                // Office/Site engineer with departments � bulk fetch teamlead status for all their depts at once
                var deptIdsWithTeamLead = await (from ed in _db.EngineerDepartments
                                                 join role in _db.Roles on ed.RoleId equals role.Id
                                                 where allEngineerDeptIds.Contains(ed.DepartmentId)
                                                       && role.Name == RoleNames.Teamleadengineer
                                                 select ed.DepartmentId)
                                                .Distinct()
                                                .ToListAsync(cancellationToken);

                // Fallback legacy check for depts without EngineerDepartments teamlead
                var legacyTeamLeadDeptIds = await (from eng in _db.Engineers
                                                   join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                                                   join role in _db.Roles on userRole.RoleId equals role.Id
                                                   where allEngineerDeptIds.Contains(eng.DepartmentId ?? Guid.Empty)
                                                         && role.Name == RoleNames.Teamleadengineer
                                                   select eng.DepartmentId!.Value)
                                                  .Distinct()
                                                  .ToListAsync(cancellationToken);

                foreach (var id in legacyTeamLeadDeptIds.Where(id => !deptIdsWithTeamLead.Contains(id)))
                    deptIdsWithTeamLead.Add(id);

                var deptsWithoutTeamLead = allEngineerDeptIds.Where(d => !deptIdsWithTeamLead.Contains(d)).ToList();
                var deptsWithTeamLead = deptIdsWithTeamLead.Where(d => allEngineerDeptIds.Contains(d)).ToList();

                query = query.Where(r =>
                    // Depts without teamlead: see unassigned + assigned to them
                    (r.DepartmentId.HasValue && deptsWithoutTeamLead.Contains(r.DepartmentId.Value)
                        && (r.assignToId == null || r.assignToId == Guid.Empty || r.assignToId == engineer.Id))
                    // Depts with teamlead: only see assigned to them
                    || (r.DepartmentId.HasValue && deptsWithTeamLead.Contains(r.DepartmentId.Value) && r.assignToId == engineer.Id)
                    || r.assignToId == engineer.Id
                    || r.EngineerId == engineer.Id);
            }
            else
            {
                // Engineer with no department: assigned to them OR created by them
                query = query.Where(r => r.assignToId == engineer.Id || r.EngineerId == engineer.Id);
            }
        }
        // Admin/SuperAdmin: no role filter � sees all requests

        // Apply optional narrowing filters regardless of role (applied after visibility scope is set above)
        if (filter.AssignToId.HasValue && filter.AssignToId.Value != Guid.Empty)
            query = query.Where(r => r.assignToId == filter.AssignToId.Value);

        if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            query = query.Where(r => r.DepartmentId == filter.DepartmentId.Value);

        // Return all without pagination at this stage - pagination happens after combining with other request types
        var requests = await query.ToListAsync(cancellationToken);

        var unifiedRequests = requests.Select(r => new GetUnifiedRequestDto
        {
            Id = r.Id,
            RequestNumber = $"ER-{r.Id}",
            RequestType = string.IsNullOrEmpty(r.RequestType) ? "EngineerRequest" : r.RequestType,
            ProjectId = r.ProjectId,
            Project = r.Project == null ? null : new GetProjectDto
            {
                Id = r.Project.Id,
                nameEn = r.Project.nameEn,
                nameAr = r.Project.nameAr
            },
            RequestedById = r.EngineerId,
            RequestedBy = r.Engineer == null ? null : new GetEngineerDto
            {
                Id = r.Engineer.Id,
                nameEn = r.Engineer.nameEn,
                nameAr = r.Engineer.nameAr
            },
            StatusId = r.StatusId,
            Status = r.Status == null ? null : new GetDropDownStatusDto
            {
                Id = r.Status.Id,
                nameEn = r.Status.nameEn,
                nameAr = r.Status.nameAr,
                Code = r.Status.Code,
                orderNumber = r.Status.orderNumber,
                iconName = r.Status.iconName
            },
            Notes = r.Descreption,
            CreatedDate = r.CreatedDate,
            DepartmentId = r.DepartmentId,
            Department = r.Department == null ? null : new GetDepartmentDto
            {
                Id = r.Department.Id,
                nameEn = r.Department.nameEn,
                nameAr = r.Department.nameAr,
                RequiresGoodsReceipt = r.Department.RequiresGoodsReceipt,
                hasSpecialFields = r.Department.hasSpecialFields
            },
            PriorityId = r.PriorityId,
            Priority = r.Priority == null ? null : new GetDropDownPriorityDto
            {
                Id = r.Priority.Id,
                nameEn = r.Priority.nameEn,
                nameAr = r.Priority.nameAr,
                code = r.Priority.code,
                iconName = r.Priority.iconName
            },
            RequestTitle = r.RequestTitle,
            Description = r.Descreption,
            AssignedToId = r.assignToId,
            AssignedTo = r.assignTo == null ? null : new GetEngineerDto
            {
                Id = r.assignTo.Id,
                nameEn = r.assignTo.nameEn,
                nameAr = r.assignTo.nameAr
            },
            StartDate = r.startDate,
            EndDate = r.endDate,
            NeedsReceiptConfirmation = r.NeedsReceiptConfirmation,
            EngineerRequestAttachments = r.EngineerRequestAttachments == null ? new() : r.EngineerRequestAttachments.Select(a => new GetAttachmentDto { Id = a.Id, Key = a.Key, FileName = a.FileName, Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url }).ToList(),
            SpecialFieldValues = r.SpecialFieldValues == null ? new() : r.SpecialFieldValues
                .OrderBy(v => v.DepartmentSpecialField?.Order)
                .Select(v => new EngineerRequestSpecialFieldValueDto
                {
                    Id = v.Id,
                    DepartmentSpecialFieldId = v.DepartmentSpecialFieldId,
                    fieldName = v.DepartmentSpecialField?.SpecialField?.name,
                    fieldType = v.DepartmentSpecialField?.SpecialField?.fieldType,
                    value = v.value,
                    Order = v.DepartmentSpecialField?.Order ?? 0,
                    ColSpan = v.DepartmentSpecialField?.ColSpan ?? 1
                }).ToList(),
            SpecialFieldItems = r.SpecialFieldItems == null ? new() : r.SpecialFieldItems.Select(i => new GetEngineerRequestSpecialFieldItemDto
            {
                Id = i.Id,
                DepartmentSpecialFieldId = i.DepartmentSpecialFieldId,
                ConstructionItemId = i.ConstructionItemId,
                Quantity = i.Quantity,
                ReceivedQuantity = i.ReceivedQuantity,
                ConstructionItem = i.ConstructionItem == null ? null : new GetConstructionItemDto
                {
                    Id = i.ConstructionItem.Id,
                    nameEn = i.ConstructionItem.nameEn,
                    nameAr = i.ConstructionItem.nameAr,
                    ItemCode = i.ConstructionItem.ItemCode,
                    Units = i.ConstructionItem.Units == null ? new() : i.ConstructionItem.Units.Select(u => new ConstructionItemUnitDto { nameEn = u.nameEn, nameAr = u.nameAr }).ToList()
                }
            }).ToList(),
            SpecialFieldListItems = r.SpecialFieldListItems == null ? new() : r.SpecialFieldListItems
                .OrderBy(i => i.RowIndex).ThenBy(i => i.DepartmentSpecialField != null ? i.DepartmentSpecialField.Order : 0)
                .Select(i => new GetEngineerRequestSpecialFieldListItemDto
                {
                    Id = i.Id,
                    DepartmentSpecialFieldId = i.DepartmentSpecialFieldId,
                    fieldName = i.DepartmentSpecialField != null && i.DepartmentSpecialField.SpecialField != null ? i.DepartmentSpecialField.SpecialField.name : null,
                    fieldType = i.DepartmentSpecialField != null && i.DepartmentSpecialField.SpecialField != null ? i.DepartmentSpecialField.SpecialField.fieldType : null,
                    RowIndex = i.RowIndex,
                    value = i.value,
                    Order = i.DepartmentSpecialField != null ? i.DepartmentSpecialField.Order : 0,
                    ListGroupKey = i.DepartmentSpecialField != null ? i.DepartmentSpecialField.ListGroupKey : null,
                    ReceivedQuantity = i.ReceivedQuantity
                }).ToList(),
            EngineerRequestNotes = r.EngineerRequestNotes == null ? new() : r.EngineerRequestNotes.Select(n => new GetEngineerRequestNotesDto
            {
                Id = n.Id,
                note = n.note,
                EngineerId = n.EngineerId,
                Engineer = n.Engineer == null ? null : new GetEngineerDto { Id = n.Engineer.Id, nameEn = n.Engineer.nameEn, nameAr = n.Engineer.nameAr },
                CreatedDate = n.CreatedDate,
                Attachments = n.EngineerRequestAttachments == null ? new List<GetAttachmentDto>() : n.EngineerRequestAttachments.Select(a => new GetAttachmentDto { Id = a.Id, Key = a.Key, FileName = a.FileName, Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url }).ToList()
            }).ToList(),
            EngineerRequestActivites = r.EngineerRequestActivites == null ? new() : r.EngineerRequestActivites.Select(a => new GetEngineerRequestActiviteDto
            {
                Id = a.Id,
                EngineerRequestId = a.EngineerRequestId,
                EngineerId = a.EngineerId,
                EngineerName = a.Engineer != null ? $"{a.Engineer.nameEn} / {a.Engineer.nameAr}" : null,
                StatusId = a.StatusId,
                StatusName = a.Status != null ? $"{a.Status.nameEn} / {a.Status.nameAr}" : null,
                ActionType = a.ActionType,
                Comments = a.Comments,
                CreatedDate = a.CreatedDate
            }).OrderByDescending(a => a.CreatedDate).ToList()
        }).ToList();

        return new PaginatedList<GetUnifiedRequestDto>(
            unifiedRequests,
            unifiedRequests.Count,
            1,
            int.MaxValue); // Return all items - pagination happens later
    }

    // ---------------- HELPER METHODS FOR UNIFIED REQUESTS ----------------

    private async Task<List<GetUnifiedRequestDto>> GetTransferRequestsForUnifiedAsync(
        Engineer engineer,
        EngineerRequestParticipationFilterDto filter,
        CancellationToken cancellationToken)
    {
        // Site Engineers and Admins can always see Transfer Requests in any status. Office
        // Engineers and Team Leads only see them once the request is Completed — they have no
        // role in the approval flow, so showing New/InProgress requests is noise to them.
        var transferRoles = CurrentUser.Roles;
        var isTransferAdmin = transferRoles.Any(r =>
            r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));
        var isSiteEngineer = transferRoles.Any(r =>
            r.Equals(RoleNames.Siteengineer, StringComparison.OrdinalIgnoreCase));
        var isOfficeOrTeamLead = !isTransferAdmin && !isSiteEngineer;

        // Get all project IDs this engineer is assigned to (destination project visibility)
        var engineerProjectIds = await _db.EngineerProjects
            .Where(ep => ep.EngineerId == engineer.Id)
            .Select(ep => ep.ProjectId)
            .ToListAsync(cancellationToken);

        // Collect all department IDs this engineer belongs to
        var engineerDeptIds = await _db.EngineerDepartments
            .Where(ed => ed.EngineerId == engineer.Id)
            .Select(ed => ed.DepartmentId)
            .ToListAsync(cancellationToken);

        if (engineer.DepartmentId.HasValue && !engineerDeptIds.Contains(engineer.DepartmentId.Value))
            engineerDeptIds.Add(engineer.DepartmentId.Value);

        // Does the engineer belong to any department with NotifyOnTransferComplete enabled?
        var hasNotifyDepts = engineerDeptIds.Any() && await _db.Departmentes
            .AnyAsync(d => engineerDeptIds.Contains(d.Id) && d.NotifyOnTransferComplete, cancellationToken);

        if (!isTransferAdmin && !isSiteEngineer && !hasNotifyDepts)
            return new List<GetUnifiedRequestDto>();

        // Resolve the engineer's branch IDs so we can gate cross-department visibility
        // to the same branch only (prevents seeing completed requests from other branches).
        var engineerBranchIds = hasNotifyDepts && engineerDeptIds.Any()
            ? await _db.Departmentes
                .Where(d => engineerDeptIds.Contains(d.Id) && !d.IsDeleted)
                .Select(d => d.BranchId)
                .Distinct()
                .ToListAsync(cancellationToken)
            : new List<Guid>();

        var query = _db.TransferRequests
            .Include(r => r.SourceProject)
            .Include(r => r.DestinationProject)
            .Include(r => r.RequestedBy)
                .ThenInclude(e => e!.Department)
            .Include(r => r.Status)
            .Include(r => r.Items)
            .Include(r => r.Attachments)
            .Include(r => r.Activities).ThenInclude(a => a.Engineer)
            .Include(r => r.Activities).ThenInclude(a => a.FromStatus)
            .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        // Load status IDs once — needed for both visibility and status filtering below.
        var ts = await StatusResolver.LoadRequestStatusIdsAsync(_db);

        if (!isTransferAdmin)
        {
            if (hasNotifyDepts)
                // Notify-dept members see:
                //   1. Their own requests (any status — they submitted it)
                //   2. Completed transfers from their branch (NOT New/InProgress — the dept is
                //      only meant to learn about a transfer after it reaches its destination)
                query = query.Where(r =>
                    r.RequestedById == engineer.Id
                    || (r.StatusId == ts.Completed && (
                        (r.SourceProject != null && r.SourceProject.BranchId.HasValue && engineerBranchIds.Contains(r.SourceProject.BranchId.Value))
                        || (r.DestinationProject != null && r.DestinationProject.BranchId.HasValue && engineerBranchIds.Contains(r.DestinationProject.BranchId.Value))
                        || (r.RequestedBy != null && r.RequestedBy.Department != null
                            && engineerBranchIds.Contains(r.RequestedBy.Department.BranchId)))));
            else
                query = query.Where(r =>
                    r.RequestedById == engineer.Id
                    || (r.DestinationProjectId.HasValue
                        && engineerProjectIds.Contains(r.DestinationProjectId.Value)));
        }

        // Office Engineers and Team Leads: explicit safety guard (the visibility condition above
        // already restricts them to Completed, but this makes the intent unambiguous).
        if (isOfficeOrTeamLead)
            query = query.Where(r => r.StatusId == ts.Completed);

        if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            query = query.Where(r => r.SourceProjectId == filter.ProjectId.Value || r.DestinationProjectId == filter.ProjectId.Value);

        // AssignToId for transfers: TransferRequest has no AssignedToId, so filter by RequestedById.
        if (filter.AssignToId.HasValue && filter.AssignToId.Value != Guid.Empty)
            query = query.Where(r => r.RequestedById == filter.AssignToId.Value);

        // Plain status filter — no NeedsAcknowledgment bypass needed because visibility is now
        // gated on StatusId==Completed (not on the transient NeedsAcknowledgment flag).
        // This ensures GetRequestsByStatus correctly shows Completed transfers even after they
        // have been acknowledged (NeedsAcknowledgment = false).
        if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
            query = query.Where(r => r.StatusId == filter.StatusId.Value);

        var requests = await query.ToListAsync(cancellationToken);

        return requests.Select(r => new GetUnifiedRequestDto
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestType = "TransferRequest",
            ProjectId = r.SourceProjectId,
            Project = r.SourceProject == null ? null : new GetProjectDto 
            { 
                Id = r.SourceProject.Id, 
                nameEn = r.SourceProject.nameEn, 
                nameAr = r.SourceProject.nameAr 
            },
            RequestedById = r.RequestedById,
            RequestedBy = r.RequestedBy == null ? null : new GetEngineerDto 
            { 
                Id = r.RequestedBy.Id, 
                nameEn = r.RequestedBy.nameEn, 
                nameAr = r.RequestedBy.nameAr 
            },
            DepartmentId = r.RequestedBy?.DepartmentId,
            Department = r.RequestedBy?.Department == null ? null : new GetDepartmentDto
            {
                Id = r.RequestedBy.Department.Id,
                nameEn = r.RequestedBy.Department.nameEn,
                nameAr = r.RequestedBy.Department.nameAr,
                RequiresGoodsReceipt = r.RequestedBy.Department.RequiresGoodsReceipt,
                hasSpecialFields = r.RequestedBy.Department.hasSpecialFields,
                NotifyOnTransferComplete = r.RequestedBy.Department.NotifyOnTransferComplete
            },
            NeedsAcknowledgment = r.NeedsAcknowledgment,
            StatusId = r.StatusId,
            Status = r.Status == null ? null : new GetDropDownStatusDto { Id = r.Status.Id, nameEn = r.Status.nameEn, nameAr = r.Status.nameAr, Code = r.Status.Code, orderNumber = r.Status.orderNumber, iconName = r.Status.iconName },
            Notes = r.Notes,
            CreatedDate = r.CreatedDate,
            SourceProjectId = r.SourceProjectId,
            SourceProject = r.SourceProject == null ? null : new GetProjectDto 
            { 
                Id = r.SourceProject.Id, 
                nameEn = r.SourceProject.nameEn, 
                nameAr = r.SourceProject.nameAr 
            },
            SourceWarehouse = r.SourceWarehouse,
            DestinationProjectId = r.DestinationProjectId,
            DestinationProject = r.DestinationProject == null ? null : new GetProjectDto 
            { 
                Id = r.DestinationProject.Id, 
                nameEn = r.DestinationProject.nameEn, 
                nameAr = r.DestinationProject.nameAr 
            },
            DestinationWarehouse = r.DestinationWarehouse,
            TransferItems = r.Items.Select(i => new GetTransferRequestItemDto
            {
                Id = i.Id,
                ItemCode = i.ItemCode,
                ItemName = i.ItemName,
                Unit = i.Unit,
                Quantity = i.Quantity,
                ReceivedQuantity = i.ReceivedQuantity,
                Notes = i.Notes
            }).ToList(),
            EngineerRequestAttachments = r.Attachments == null ? new() : r.Attachments.Select(a => new GetAttachmentDto { Id = a.Id, Key = a.Key, FileName = a.FileName, Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url }).ToList(),
            EngineerRequestActivites = r.Activities == null ? new() : r.Activities.Select(a => new GetEngineerRequestActiviteDto
            {
                Id = a.Id,
                EngineerRequestId = r.Id,
                EngineerId = a.EngineerId,
                EngineerName = a.Engineer != null ? $"{a.Engineer.nameEn} / {a.Engineer.nameAr}" : null,
                StatusId = a.ToStatusId,
                StatusName = a.ToStatus != null ? $"{a.ToStatus.nameEn} / {a.ToStatus.nameAr}" : null,
                ActionType = a.ActionType,
                Comments = a.Comments,
                CreatedDate = a.CreatedDate
            }).OrderByDescending(a => a.CreatedDate).ToList(),
            EngineerRequestNotes = r.Activities == null ? new() : r.Activities
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
                .ToList()
        }).ToList();
    }

    private async Task<List<GetUnifiedRequestDto>> GetLaborAttendanceRequestsForUnifiedAsync(
        Engineer engineer,
        EngineerRequestParticipationFilterDto filter,
        CancellationToken cancellationToken)
    {
        // Get roles to check if admin
        var roles = CurrentUser.Roles;
        var isAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                  || r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        // Resolved once for NotifyAfterLaborApprove visibility
        var s = await StatusResolver.LoadRequestStatusIdsAsync(_db);

        var query = _db.LaborAttendanceRequests
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.Supervisor)
            .Include(r => r.AssignedTo)
            .Include(r => r.Status)
            .Include(r => r.Records)
            .Include(r => r.Attachments)
            .Include(r => r.Activities).ThenInclude(a => a.Engineer)
            .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
        {
            query = query.Where(r => r.ProjectId == filter.ProjectId.Value);
        }

        if (!isAdmin)
        {
            // Get all departments where engineer is a TeamLead
            var teamLeadDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineer.Id && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
                .Select(ed => ed.DepartmentId)
                .ToListAsync(cancellationToken);

            // Fallback: check via legacy DepartmentId + global UserRoles
            if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
            {
                var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                join role in _db.Roles on userRole.RoleId equals role.Id
                                                where userRole.UserId == engineer.ApplicationUserId
                                                      && role.Name == RoleNames.Teamleadengineer
                                                select role.Id).AnyAsync(cancellationToken);
                if (isTeamLeadViaRoles)
                    teamLeadDeptIds.Add(engineer.DepartmentId.Value);
            }

            // If DepartmentId filter is provided, narrow down
            if (filter.DepartmentId.HasValue && filter.DepartmentId.Value != Guid.Empty)
            {
                teamLeadDeptIds = teamLeadDeptIds.Where(d => d == filter.DepartmentId.Value).ToList();
            }

            bool isTeamLead = teamLeadDeptIds.Any();

            // Is this engineer in any department with NotifyAfterLaborApprove = true?
            var isInNotifyLaborDept = await _db.EngineerDepartments
                .AnyAsync(ed => ed.EngineerId == engineer.Id && ed.Department!.NotifyAfterLaborApprove && !ed.Department.IsDeleted, cancellationToken);
            if (!isInNotifyLaborDept && engineer.DepartmentId.HasValue)
                isInNotifyLaborDept = await _db.Departmentes
                    .AnyAsync(d => d.Id == engineer.DepartmentId.Value && d.NotifyAfterLaborApprove && !d.IsDeleted, cancellationToken);

            if (isTeamLead)
            {
                query = query.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.AssignedToId == engineer.Id
                    || r.SupervisorId == engineer.Id
                    || (isInNotifyLaborDept && r.StatusId == s.Completed));
            }
            else
            {
                query = query.Where(r =>
                    r.SupervisorId == engineer.Id
                    || r.AssignedToId == engineer.Id
                    || (isInNotifyLaborDept && r.StatusId == s.Completed));
            }
        }
        // Admin/SuperAdmin: sees all requests (only filtered by data params above)

        if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
            query = query.Where(r => r.StatusId == filter.StatusId.Value);

        if (filter.AssignToId.HasValue && filter.AssignToId.Value != Guid.Empty)
            query = query.Where(r => r.AssignedToId == filter.AssignToId.Value);

        var requests = await query.ToListAsync(cancellationToken);

        return requests.Select(r => new GetUnifiedRequestDto
        {
            Id = r.Id,
            RequestNumber = r.RequestNumber,
            RequestType = "LaborAttendance",
            ProjectId = r.ProjectId,
            Project = r.Project == null ? null : new GetProjectDto 
            { 
                Id = r.Project.Id, 
                nameEn = r.Project.nameEn, 
                nameAr = r.Project.nameAr 
            },
            DepartmentId = r.DepartmentId,
            Department = r.Department == null ? null : new GetDepartmentDto 
            { 
                Id = r.Department.Id, 
                nameEn = r.Department.nameEn, 
                nameAr = r.Department.nameAr,
                RequiresGoodsReceipt = r.Department.RequiresGoodsReceipt,
                hasSpecialFields = r.Department.hasSpecialFields
            },
            RequestedById = r.SupervisorId,
            RequestedBy = r.Supervisor == null ? null : new GetEngineerDto 
            { 
                Id = r.Supervisor.Id, 
                nameEn = r.Supervisor.nameEn, 
                nameAr = r.Supervisor.nameAr 
            },
            AssignedToId = r.AssignedToId,
            AssignedTo = r.AssignedTo == null ? null : new GetEngineerDto 
            { 
                Id = r.AssignedTo.Id, 
                nameEn = r.AssignedTo.nameEn, 
                nameAr = r.AssignedTo.nameAr 
            },
            StatusId = r.StatusId,
            Status = r.Status == null ? null : new GetDropDownStatusDto { Id = r.Status.Id, nameEn = r.Status.nameEn, nameAr = r.Status.nameAr, Code = r.Status.Code, orderNumber = r.Status.orderNumber, iconName = r.Status.iconName },
            Notes = r.Notes,
            CreatedDate = r.CreatedDate,
            SiteName = r.SiteName,
            AttendanceDate = r.AttendanceDate,
            SupervisorId = r.SupervisorId,
            Supervisor = r.Supervisor == null ? null : new GetEngineerDto 
            { 
                Id = r.Supervisor.Id, 
                nameEn = r.Supervisor.nameEn, 
                nameAr = r.Supervisor.nameAr 
            },
            TotalAmount = r.Records.Sum(rec => rec.TotalAmount),
            // Include worker/labor records for finance department review
            LaborRecords = r.Records.Select(rec => new GetLaborAttendanceRecordDto
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
            EngineerRequestAttachments = r.Attachments == null ? new() : r.Attachments.Select(a => new GetAttachmentDto { Id = a.Id, Key = a.Key, FileName = a.FileName, Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url }).ToList(),
            EngineerRequestActivites = r.Activities == null ? new() : r.Activities.Select(a => new GetEngineerRequestActiviteDto
            {
                Id = a.Id,
                EngineerRequestId = r.Id,
                EngineerId = a.EngineerId,
                EngineerName = a.Engineer != null ? $"{a.Engineer.nameEn} / {a.Engineer.nameAr}" : null,
                StatusId = a.ToStatusId,
                StatusName = a.ToStatus != null ? $"{a.ToStatus.nameEn} / {a.ToStatus.nameAr}" : null,
                ActionType = a.ActionType,
                Comments = a.Comments,
                CreatedDate = a.CreatedDate
            }).OrderByDescending(a => a.CreatedDate).ToList(),
            EngineerRequestNotes = r.Activities == null ? new() : r.Activities
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
                .ToList()
        }).ToList();
    }

    private async Task<List<GetUnifiedRequestDto>> GetFinancialClearancesForUnifiedAsync(
        Engineer engineer,
        EngineerRequestParticipationFilterDto filter,
        CancellationToken cancellationToken)
    {
        var query = _db.FinancialClearances
            .Include(r => r.Project)
            .Include(r => r.Department)
            .Include(r => r.RequestedBy)
            .Include(r => r.AssignedTo)
            .Include(r => r.Status)
            .Include(r => r.Items)
            .Include(r => r.Attachments)
            .Include(r => r.Activities).ThenInclude(a => a.Engineer)
            .Include(r => r.Activities).ThenInclude(a => a.ToStatus)
            .Where(r => !r.IsDeleted)
            .AsNoTracking();

        // Role-based visibility (mirrors GetLaborAttendanceRequestsForUnifiedAsync):
        // admins see all; team leads see their departments' requests (so they can assign
        // them) plus ones assigned to / created by them; everyone else sees only their own
        // requests or requests assigned to them.
        var financialRoles = CurrentUser.Roles;
        var isFinancialAdmin = financialRoles.Any(r =>
            r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase) ||
            r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        if (!isFinancialAdmin)
        {
            var fcStatus = await StatusResolver.LoadRequestStatusIdsAsync(_db);

            var teamLeadDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineer.Id && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
                .Select(ed => ed.DepartmentId)
                .ToListAsync(cancellationToken);

            if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
            {
                var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                join role in _db.Roles on userRole.RoleId equals role.Id
                                                where userRole.UserId == engineer.ApplicationUserId
                                                      && role.Name == RoleNames.Teamleadengineer
                                                select role.Id).AnyAsync(cancellationToken);
                if (isTeamLeadViaRoles)
                    teamLeadDeptIds.Add(engineer.DepartmentId.Value);
            }

            // Is this engineer in any department with NotifyAfterFinancialClearanceApprove = true?
            var isInNotifyFCDept = await _db.EngineerDepartments
                .AnyAsync(ed => ed.EngineerId == engineer.Id && ed.Department!.NotifyAfterFinancialClearanceApprove && !ed.Department.IsDeleted, cancellationToken);
            if (!isInNotifyFCDept && engineer.DepartmentId.HasValue)
                isInNotifyFCDept = await _db.Departmentes
                    .AnyAsync(d => d.Id == engineer.DepartmentId.Value && d.NotifyAfterFinancialClearanceApprove && !d.IsDeleted, cancellationToken);

            if (teamLeadDeptIds.Any())
                query = query.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.AssignedToId == engineer.Id
                    || r.RequestedById == engineer.Id
                    || (isInNotifyFCDept && r.StatusId == fcStatus.Completed));
            else
                query = query.Where(r =>
                    r.RequestedById == engineer.Id
                    || r.AssignedToId == engineer.Id
                    || (isInNotifyFCDept && r.StatusId == fcStatus.Completed));
        }

        if (filter.ProjectId.HasValue && filter.ProjectId.Value != Guid.Empty)
            query = query.Where(r => r.ProjectId == filter.ProjectId.Value);

        if (filter.StatusId.HasValue && filter.StatusId.Value != Guid.Empty)
            query = query.Where(r => r.StatusId == filter.StatusId.Value);

        if (filter.AssignToId.HasValue && filter.AssignToId.Value != Guid.Empty)
            query = query.Where(r => r.AssignedToId == filter.AssignToId.Value);

        var requests = await query.ToListAsync(cancellationToken);

        return requests.Select(r => new GetUnifiedRequestDto
        {
            Id = r.Id,
            RequestNumber = r.ClearanceNumber,
            RequestType = "FinancialClearance",
            ProjectId = r.ProjectId,
            Project = r.Project == null ? null : new GetProjectDto 
            { 
                Id = r.Project.Id, 
                nameEn = r.Project.nameEn, 
                nameAr = r.Project.nameAr 
            },
            RequestedById = r.RequestedById,
            RequestedBy = r.RequestedBy == null ? null : new GetEngineerDto
            {
                Id = r.RequestedBy.Id,
                nameEn = r.RequestedBy.nameEn,
                nameAr = r.RequestedBy.nameAr
            },
            AssignedToId = r.AssignedToId,
            AssignedTo = r.AssignedTo == null ? null : new GetEngineerDto
            {
                Id = r.AssignedTo.Id,
                nameEn = r.AssignedTo.nameEn,
                nameAr = r.AssignedTo.nameAr
            },
            StatusId = r.StatusId,
            Status = r.Status == null ? null : new GetDropDownStatusDto { Id = r.Status.Id, nameEn = r.Status.nameEn, nameAr = r.Status.nameAr, Code = r.Status.Code, orderNumber = r.Status.orderNumber, iconName = r.Status.iconName },
            Notes = r.Notes,
            CreatedDate = r.CreatedDate,
            DepartmentId = r.DepartmentId,
            Department = r.Department == null ? null : new GetDepartmentDto
            {
                Id = r.Department.Id,
                nameEn = r.Department.nameEn,
                nameAr = r.Department.nameAr,
                RequiresGoodsReceipt = r.Department.RequiresGoodsReceipt,
                hasSpecialFields = r.Department.hasSpecialFields
            },
            StartDate = r.FromDate,
            EndDate = r.ToDate,
            ClearanceNumber = r.ClearanceNumber,
            EmployeeName = r.EmployeeName,
            AdvanceAmount = r.AdvanceAmount,
            SpentAmount = r.SpentAmount,
            RemainingAmount = r.RemainingAmount,
            FinancialClearanceItems = r.Items == null ? new() : r.Items.Select(i => new GetFinancialClearanceItemDto { Id = i.Id, Code = i.Code, ItemName = i.ItemName, Description = i.Description, Value = i.Value }).ToList(),
            EngineerRequestAttachments = r.Attachments == null ? new() : r.Attachments.Select(a => new GetAttachmentDto { Id = a.Id, Key = a.Key, FileName = a.FileName, Extension = a.Extension, FileSize = a.FileSize, Url = _storageService.GetPreSignedUrl(a.Key) ?? a.Url }).ToList(),
            EngineerRequestActivites = r.Activities == null ? new() : r.Activities.Select(a => new GetEngineerRequestActiviteDto
            {
                Id = a.Id,
                EngineerRequestId = r.Id,
                EngineerId = a.EngineerId,
                EngineerName = a.Engineer != null ? $"{a.Engineer.nameEn} / {a.Engineer.nameAr}" : null,
                StatusId = a.ToStatusId,
                StatusName = a.ToStatus != null ? $"{a.ToStatus.nameEn} / {a.ToStatus.nameAr}" : null,
                ActionType = a.ActionType,
                Comments = a.Comments,
                CreatedDate = a.CreatedDate
            }).OrderByDescending(a => a.CreatedDate).ToList()
        }).ToList();
    }

    // ---------------- GET REQUESTS BY STATUS FOR ENGINEER ----------------
    public async Task<GetRequestsByStatusResponseDto> GetRequestsByStatusForEngineerAsync(
        GetRequestsByStatusFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var empty = new GetRequestsByStatusResponseDto { PageNumber = filter.PageIndex, PageSize = filter.PageSize };
        try
        {
            // Reuse the unified helpers with a status filter — mirrors GetCreatedRequestOrapplaied
            var participationFilter = new EngineerRequestParticipationFilterDto
            {
                PageIndex  = 1,
                PageSize   = int.MaxValue,
                StatusId   = filter.StatusId
            };

            // EngineerRequests (role-filtered, includes InternalRequest rows)
            var engineerPage = await GetEngineerRequestsWithOriginalLogicAsync(participationFilter, cancellationToken);
            var all = engineerPage.Items.ToList();

            var engineer = await _db.Engineers
                .Include(x => x.ApplicationUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == Guid.Parse(CurrentUser.UserId), cancellationToken);

            if (engineer != null)
            {
                var transfers = await GetTransferRequestsForUnifiedAsync(engineer, participationFilter, cancellationToken);
                all.AddRange(transfers);

                var labor = await GetLaborAttendanceRequestsForUnifiedAsync(engineer, participationFilter, cancellationToken);
                all.AddRange(labor);

                var financial = await GetFinancialClearancesForUnifiedAsync(engineer, participationFilter, cancellationToken);
                all.AddRange(financial);
            }

            all = all.OrderByDescending(r => r.CreatedDate).ToList();

            int erCount       = all.Count(r => r.RequestType == "EngineerRequest" || r.RequestType == "InternalRequest");
            int transferCount = all.Count(r => r.RequestType == "TransferRequest");
            int laborCount    = all.Count(r => r.RequestType == "LaborAttendance");
            int financialCount= all.Count(r => r.RequestType == "FinancialClearance");
            int totalCount    = all.Count;

            var paged = all
                .Skip((filter.PageIndex - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            int totalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize);
            return new GetRequestsByStatusResponseDto
            {
                Items                   = paged,
                PageNumber              = filter.PageIndex,
                PageSize                = filter.PageSize,
                TotalPages              = totalPages,
                TotalCount              = totalCount,
                FirstItemOnPage         = totalCount == 0 ? 0 : (filter.PageIndex - 1) * filter.PageSize + 1,
                LastItemOnPage          = Math.Min(filter.PageIndex * filter.PageSize, totalCount),
                IsFirstPage             = filter.PageIndex == 1,
                IsLastPage              = filter.PageIndex >= totalPages,
                EngineerRequestCount    = erCount,
                TransferCount           = transferCount,
                LaborCount              = laborCount,
                FinancialClearanceCount = financialCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetRequestsByStatusForEngineerAsync failed for user {UserId}", CurrentUser.UserId);
            return empty;
        }
    }

    public async Task<bool> DepartmentHasTeamLeadAsync(Guid departmentId)
    {
        var teamleadRoleName = RoleNames.Teamleadengineer;

        // Check via EngineerDepartments (multi-department role assignment)
        var hasViaDepRole = await _db.EngineerDepartments
            .AnyAsync(ed => ed.DepartmentId == departmentId
                         && ed.Role != null && ed.Role.Name == teamleadRoleName);

        if (hasViaDepRole) return true;

        // Fallback: check via legacy DepartmentId + global UserRoles
        return await (from eng in _db.Engineers
                      join userRole in _db.UserRoles on eng.ApplicationUserId equals userRole.UserId
                      join role in _db.Roles on userRole.RoleId equals role.Id
                      where eng.DepartmentId == departmentId && role.Name == teamleadRoleName
                      select eng.Id).AnyAsync();
    }

    // ---------------- GET REQUEST ACTIVITIES ----------------
    public async Task<List<GetEngineerRequestActiviteDto>> GetRequestActivitiesAsync(Guid requestId)
    {
        var activities = await _db.EngineerRequestActivites
            .Include(a => a.Engineer)
            .Include(a => a.Status)
            .Where(a => a.EngineerRequestId == requestId)
            .OrderByDescending(a => a.CreatedDate)
            .AsNoTracking()
            .ToListAsync();

        var activitiesDto = activities.Select(a => new GetEngineerRequestActiviteDto
        {
            Id = a.Id,
            EngineerRequestId = a.EngineerRequestId,
            EngineerId = a.EngineerId,
            EngineerName = a.Engineer != null ? $"{a.Engineer.nameEn} / {a.Engineer.nameAr}" : null,
            StatusId = a.StatusId,
            StatusName = a.Status != null ? $"{a.Status.nameEn} / {a.Status.nameAr}" : null,
            ActionType = a.ActionType,
            Comments = a.Comments,
            CreatedDate = a.CreatedDate
        }).ToList();

        return activitiesDto;
    }

    // ---------------- GET ENGINEER REQUEST COUNT BY STATUS ----------------
    public async Task<List<GetEngineerRequestCountByStatusDto>> GetEngineerRequestCountByStatusAsync(Guid engineerId)
    {
        var roles = CurrentUser.Roles;
        var isAdmin = roles.Any(r => r.Equals(RoleNames.SuperAdmin, StringComparison.OrdinalIgnoreCase)
                                  || r.Equals(RoleNames.Admin, StringComparison.OrdinalIgnoreCase));

        // Load engineer — may be null for SuperAdmin with no Engineer record
        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == engineerId);

        var teamLeadDeptIds = new List<Guid>();
        var allEngineerDeptIds = new List<Guid>();

        if (engineer is not null)
        {
            // Get all departments where engineer is a TeamLead (via EngineerDepartments)
            teamLeadDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineerId && ed.Role != null && ed.Role.Name == RoleNames.Teamleadengineer)
                .Select(ed => ed.DepartmentId)
                .ToListAsync();

            // Fallback: check via legacy DepartmentId + global UserRoles
            if (!teamLeadDeptIds.Any() && engineer.DepartmentId.HasValue)
            {
                var isTeamLeadViaRoles = await (from userRole in _db.UserRoles
                                                join role in _db.Roles on userRole.RoleId equals role.Id
                                                where userRole.UserId == engineer.ApplicationUserId
                                                      && role.Name == RoleNames.Teamleadengineer
                                                select role.Id).AnyAsync();
                if (isTeamLeadViaRoles)
                    teamLeadDeptIds.Add(engineer.DepartmentId.Value);
            }

            // Get all departments this engineer belongs to
            allEngineerDeptIds = await _db.EngineerDepartments
                .Where(ed => ed.EngineerId == engineerId)
                .Select(ed => ed.DepartmentId)
                .ToListAsync();

            // Include the active department if set
            if (engineer.DepartmentId.HasValue && !allEngineerDeptIds.Contains(engineer.DepartmentId.Value))
                allEngineerDeptIds.Add(engineer.DepartmentId.Value);
        }

        bool isTeamLead = teamLeadDeptIds.Any();
        bool isSiteEngineerForTransfer = roles.Any(r => r.Equals(RoleNames.Siteengineer, StringComparison.OrdinalIgnoreCase));

        // ── EngineerRequest count (all types — same visibility as GetRequestsByStatusForEngineerAsync) ──
        IQueryable<EngineerRequest> baseQuery = _db.EngineerRequests
            .Where(er => !er.IsDeleted);

        if (!isAdmin)
        {
            if (isTeamLead)
            {
                baseQuery = baseQuery.Where(er =>
                    (er.DepartmentId.HasValue && teamLeadDeptIds.Contains(er.DepartmentId.Value))
                    || er.assignToId == engineerId
                    || er.EngineerId == engineerId);
            }
            else if (allEngineerDeptIds.Any())
            {
                var deptsWithoutTeamLead = new List<Guid>();
                foreach (var deptId in allEngineerDeptIds)
                {
                    if (!await DepartmentHasTeamLeadAsync(deptId))
                        deptsWithoutTeamLead.Add(deptId);
                }
                var deptsWithTeamLead = allEngineerDeptIds.Where(d => !deptsWithoutTeamLead.Contains(d)).ToList();

                baseQuery = baseQuery.Where(er =>
                    (er.DepartmentId.HasValue && deptsWithoutTeamLead.Contains(er.DepartmentId.Value)
                        && (er.assignToId == null || er.assignToId == Guid.Empty || er.assignToId == engineerId))
                    || (er.DepartmentId.HasValue && deptsWithTeamLead.Contains(er.DepartmentId.Value) && er.assignToId == engineerId)
                    || er.assignToId == engineerId
                    || er.EngineerId == engineerId);
            }
            else
            {
                baseQuery = baseQuery.Where(er => er.EngineerId == engineerId || er.assignToId == engineerId);
            }
        }

        var engineerRequestGroups = await baseQuery
            .GroupBy(er => er.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        // Get all statuses
        var allStatuses = await _db.Statuses
            .AsNoTracking()
            .Select(s => new { s.Id, s.nameEn, s.nameAr })
            .ToListAsync();

        var erDict = engineerRequestGroups.ToDictionary(x => x.StatusId, x => x.Count);

        var requestCounts = allStatuses
            .Select(s => new GetEngineerRequestCountByStatusDto
            {
                StatusId     = s.Id,
                StatusName   = s.nameEn,
                StatusNameAr = s.nameAr ?? string.Empty,
                Count        = erDict.ContainsKey(s.Id) ? erDict[s.Id] : 0
            })
            .OrderBy(x => x.StatusName)
            .ToList();

        // --- Transfer Request counts ---
        // Mirrors GetTransferRequestsForUnifiedAsync exactly, INCLUDING its early "not visible at
        // all" gate: Site Engineers and Admins always see transfers; anyone else only sees them if
        // they belong to a NotifyOnTransferComplete department. A Team Lead/Office Engineer with no
        // such department gets zero transfers here, same as the appliedOrCreatedReqeust list — that
        // early gate must run BEFORE the RequestedById/destination-project checks below, otherwise
        // this count can include transfers the unified list would never actually return.
        var hasNotifyDeptsForCount = allEngineerDeptIds.Any() && await _db.Departmentes
            .AnyAsync(d => allEngineerDeptIds.Contains(d.Id) && d.NotifyOnTransferComplete);

        if (isAdmin || isSiteEngineerForTransfer || hasNotifyDeptsForCount)
        {
            var transferProjectIds = await _db.EngineerProjects
                .Where(ep => ep.EngineerId == engineerId)
                .Select(ep => ep.ProjectId)
                .ToListAsync();

            var transferBranchIds = hasNotifyDeptsForCount
                ? await _db.Departmentes
                    .Where(d => allEngineerDeptIds.Contains(d.Id) && !d.IsDeleted)
                    .Select(d => d.BranchId)
                    .Distinct()
                    .ToListAsync()
                : new List<Guid>();

            IQueryable<TransferRequest> transferQuery = _db.TransferRequests
                .Where(r => !r.IsDeleted);

            var tsForCount = await StatusResolver.LoadRequestStatusIdsAsync(_db);

            if (!isAdmin)
            {
                if (hasNotifyDeptsForCount)
                    // Mirror the updated GetTransferRequestsForUnifiedAsync: notify-dept members
                    // count their own requests (any status) + Completed transfers in their branch.
                    transferQuery = transferQuery.Where(r =>
                        r.RequestedById == engineerId
                        || (r.StatusId == tsForCount.Completed && (
                            (r.SourceProject != null && r.SourceProject.BranchId.HasValue && transferBranchIds.Contains(r.SourceProject.BranchId.Value))
                            || (r.DestinationProject != null && r.DestinationProject.BranchId.HasValue && transferBranchIds.Contains(r.DestinationProject.BranchId.Value))
                            || (r.RequestedBy != null && r.RequestedBy.Department != null && transferBranchIds.Contains(r.RequestedBy.Department.BranchId)))));
                else
                    transferQuery = transferQuery.Where(r =>
                        r.RequestedById == engineerId
                        || (r.DestinationProjectId.HasValue && transferProjectIds.Contains(r.DestinationProjectId.Value)));
            }

            var transferGroups = await transferQuery
                .GroupBy(r => r.StatusId)
                .Select(g => new { StatusId = g.Key, Count = g.Count() })
                .ToListAsync();

            foreach (var tg in transferGroups)
            {
                var existing = requestCounts.FirstOrDefault(x => x.StatusId == tg.StatusId);
                if (existing is not null)
                    existing.TransferCount += tg.Count;
                else if (tg.StatusId.HasValue)
                    requestCounts.Add(new GetEngineerRequestCountByStatusDto
                    {
                        StatusId = tg.StatusId.Value,
                        TransferCount = tg.Count
                    });
            }
        }

        // --- Labor Attendance counts ---
        // Mirrors GetLaborAttendanceRequestsForUnifiedAsync: admins see all, team leads
        // see their departments' requests (plus supervised/assigned), everyone else sees
        // only requests they supervise or are assigned to.
        IQueryable<LaborAttendanceRequest> laborQuery = _db.LaborAttendanceRequests
            .Where(r => !r.IsDeleted);

        if (!isAdmin)
        {
            if (isTeamLead)
                laborQuery = laborQuery.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.AssignedToId == engineerId
                    || r.SupervisorId == engineerId);
            else
                laborQuery = laborQuery.Where(r =>
                    r.SupervisorId == engineerId || r.AssignedToId == engineerId);
        }

        var laborGroups = await laborQuery
            .GroupBy(r => r.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var lg in laborGroups)
        {
            var existing = requestCounts.FirstOrDefault(x => x.StatusId == lg.StatusId);
            if (existing is not null)
                existing.LaborCount += lg.Count;
            else if (lg.StatusId.HasValue)
                requestCounts.Add(new GetEngineerRequestCountByStatusDto
                {
                    StatusId = lg.StatusId.Value,
                    LaborCount = lg.Count
                });
        }

        // --- Financial Clearance counts ---
        // Mirrors GetFinancialClearancesForUnifiedAsync: admins see all; team leads see their
        // departments' clearances plus ones assigned/requested by them; everyone else sees only
        // ones they requested OR are assigned to.
        IQueryable<FinancialClearance> financialQuery = _db.FinancialClearances.Where(r => !r.IsDeleted);

        if (!isAdmin)
        {
            if (isTeamLead)
                financialQuery = financialQuery.Where(r =>
                    (r.DepartmentId.HasValue && teamLeadDeptIds.Contains(r.DepartmentId.Value))
                    || r.AssignedToId == engineerId
                    || r.RequestedById == engineerId);
            else
                financialQuery = financialQuery.Where(r =>
                    r.RequestedById == engineerId || r.AssignedToId == engineerId);
        }

        var financialGroups = await financialQuery
            .GroupBy(r => r.StatusId)
            .Select(g => new { StatusId = g.Key, Count = g.Count() })
            .ToListAsync();

        foreach (var fg in financialGroups)
        {
            var existing = requestCounts.FirstOrDefault(x => x.StatusId == fg.StatusId);
            if (existing is not null)
                existing.FinancialClearanceCount += fg.Count;
            else if (fg.StatusId.HasValue)
                requestCounts.Add(new GetEngineerRequestCountByStatusDto
                {
                    StatusId = fg.StatusId.Value,
                    FinancialClearanceCount = fg.Count
                });
        }

        return requestCounts;
    }

    public async Task ProcessScheduledStatusUpdatesAsync(CancellationToken cancellationToken = default)
    {
        var now = Contracting.Shared.Common.DateTimeHelper.DateTimeNow;

        var statuses = await _db.Statuses
            .AsNoTracking()
            .Select(s => new StatusKeywordProjection(s.Id, s.nameEn, s.nameAr, s.Code))
            .ToListAsync(cancellationToken);

        var inProgressStatusId = FindStatusIdByKeywords(statuses, InProgressKeywords);
        var delayedStatusId = FindStatusIdByKeywords(statuses, DelayedKeywords);
        var completedStatusIds = ExtractStatusIds(statuses, CompletedKeywords);
        var newPendingStatusIds = ExtractStatusIds(statuses, NewPendingKeywords);

        var activities = new List<EngineerRequestActivite>();

        // Move requests to InProgress when they have planning dates.
        // This covers requests whose start date has arrived and also requests with future planned dates.
        if (inProgressStatusId.HasValue && newPendingStatusIds.Count > 0)
        {
            var startCandidates = await _db.EngineerRequests
                .Where(r => r.startDate.HasValue
                            || (r.endDate.HasValue && r.endDate.Value > now))
                .Where(r => newPendingStatusIds.Contains(r.StatusId))
                .Where(r => r.startDate.HasValue
                            || r.endDate.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var request in startCandidates)
            {
                request.StatusId = inProgressStatusId.Value;
                activities.Add(CreateAutomaticActivity(request, inProgressStatusId.Value));
            }
        }

        // For endDate: Only change to Delayed if current status is InProgress
        // This ensures we only auto-update once when the end date passes
        // If user changes status after delay, it won't be changed back to Delayed
        if (delayedStatusId.HasValue && inProgressStatusId.HasValue)
        {
            var delayCandidates = await _db.EngineerRequests
                .Where(r => r.endDate.HasValue
                            && r.endDate.Value <= now
                            && r.StatusId == inProgressStatusId.Value
                            && !completedStatusIds.Contains(r.StatusId))
                .ToListAsync(cancellationToken);

            foreach (var request in delayCandidates)
            {
                request.StatusId = delayedStatusId.Value;
                activities.Add(CreateAutomaticActivity(request, delayedStatusId.Value));
            }
        }

        if (activities.Count == 0)
        {
            return;
        }

        await _db.EngineerRequestActivites.AddRangeAsync(activities, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
    }

    private static EngineerRequestActivite CreateAutomaticActivity(EngineerRequest request, Guid statusId)
    {
        return new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = request.assignToId ?? request.EngineerId,
            StatusId = statusId,
            ActionType = EngineerRequestActionType.StatusChangedAuto.ToString()
        };
    }

    private static Guid? FindStatusIdByKeywords(IEnumerable<StatusKeywordProjection> statuses, string[] keywords)
    {
        return statuses
            .FirstOrDefault(s => HasKeyword(s.Code, keywords)
                              || HasKeyword(s.NameEn, keywords)
                              || HasKeyword(s.NameAr, keywords))?.Id;
    }

    private static HashSet<Guid> ExtractStatusIds(IEnumerable<StatusKeywordProjection> statuses, string[] keywords)
    {
        return statuses
            .Where(s => HasKeyword(s.Code, keywords)
                     || HasKeyword(s.NameEn, keywords)
                     || HasKeyword(s.NameAr, keywords))
            .Select(s => s.Id)
            .ToHashSet();
    }

    private static bool HasKeyword(string? value, string[] keywords)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        var normalized = value.Trim().ToLowerInvariant();
        return keywords.Any(keyword => normalized.Contains(keyword));
    }

    private sealed record StatusKeywordProjection(Guid Id, string? NameEn, string? NameAr, string? Code);

    public async Task<ErrorOr<bool>> ConfirmDeliveryDateAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return Error.NotFound("Request.NotFound", _localizer[SharedResourcesKeys.RequestNotFound]);

        if (request.endDate is null)
            return Error.Validation("Request.NoDeliveryDate", "Cannot confirm delivery date: no end date is set on this request.");

        if (request.IsDeliveryDateConfirmed)
            return Error.Conflict("Request.AlreadyConfirmed", "Delivery date is already confirmed.");

        request.IsDeliveryDateConfirmed = true;
        await _db.SaveChangesAsync();
        return true;
    }

    // ===== GOODS RECEIPT =====

    private static readonly string[] PurchaseClosedKeywords =
        { "closed", "completed", "complete", "done", "finished", "finish" };

    /// <summary>
    /// Matches submitted goods-receipt line items against a request's <see cref="EngineerRequestSpecialFieldItem"/> rows
    /// and applies the received quantity on each matched row.
    /// Each submitted <c>ItemId</c> may refer to either:
    ///   1) the row's own <c>Id</c> (preferred — always unambiguous), or
    ///   2) that row's <c>ConstructionItemId</c> (fallback — only accepted when exactly one row on the
    ///      request carries that ConstructionItemId, otherwise it is ambiguous and treated as unresolved).
    /// This tolerates clients that naturally send the material/ConstructionItem id instead of the
    /// EngineerRequestSpecialFieldItem row id, without ever silently dropping an update.
    /// Returns the submitted ItemIds that could not be resolved to exactly one row (empty when everything matched).
    /// </summary>
    private static List<Guid> ApplySpecialFieldItemReceivedQuantities(
        IEnumerable<EngineerRequestSpecialFieldItem> requestItems,
        IEnumerable<EngineerRequestSpecialFieldItemReceiptDto> submittedItems)
    {
        var itemsById = requestItems.ToDictionary(i => i.Id);
        var itemsByConstructionItemId = requestItems
            .GroupBy(i => i.ConstructionItemId)
            .ToDictionary(g => g.Key, g => g.ToList());

        var unresolved = new List<Guid>();

        foreach (var submitted in submittedItems)
        {
            EngineerRequestSpecialFieldItem? match = null;

            if (itemsById.TryGetValue(submitted.ItemId, out var byIdMatch))
            {
                // Unambiguous: matched by the row's own primary key.
                match = byIdMatch;
            }
            else if (itemsByConstructionItemId.TryGetValue(submitted.ItemId, out var candidates)
                     && candidates.Count == 1)
            {
                // Fallback: matched by ConstructionItemId, only accepted when there's a single
                // candidate row on this request — otherwise which row to update is ambiguous.
                match = candidates[0];
            }

            if (match is null)
            {
                unresolved.Add(submitted.ItemId);
                continue;
            }

            match.ReceivedQuantity = Math.Max(0, Math.Min(submitted.ReceivedQuantity, match.Quantity));
        }

        return unresolved;
    }

    /// <summary>
    /// Same purpose as <see cref="ApplySpecialFieldItemReceivedQuantities"/>, but for generic
    /// list-group special fields (e.g. a "Qty Required" cell in a Procurement materials row
    /// configured via ListGroupKey instead of the ConstructionItem catalog). These rows have no
    /// ConstructionItemId, so ItemId must match the row's own Id - no fallback matching.
    /// The row's own <c>value</c> (parsed as an int, when possible) is treated as "ordered quantity"
    /// and clamps the received amount, mirroring how ConstructionItem rows clamp against Quantity.
    /// Returns the submitted ItemIds that could not be resolved (empty when everything matched).
    /// </summary>
    private static List<Guid> ApplySpecialFieldListItemReceivedQuantities(
        IEnumerable<EngineerRequestSpecialFieldListItem> requestItems,
        IEnumerable<EngineerRequestSpecialFieldItemReceiptDto> submittedItems)
    {
        var itemsById = requestItems.ToDictionary(i => i.Id);
        var unresolved = new List<Guid>();

        foreach (var submitted in submittedItems)
        {
            if (!itemsById.TryGetValue(submitted.ItemId, out var match))
            {
                unresolved.Add(submitted.ItemId);
                continue;
            }

            var receivedQty = Math.Max(0, submitted.ReceivedQuantity);
            match.ReceivedQuantity = int.TryParse(match.value, out var orderedQty)
                ? Math.Min(receivedQty, orderedQty)
                : receivedQty;
        }

        return unresolved;
    }

    public async Task<ErrorOr<GetAllEngineerRequestDto>> CreateGoodsReceiptAsync(
        Guid requestId,
        CreateGoodsReceiptDto dto)
    {
        var request = await _db.EngineerRequests
            .Include(r => r.Status)
            .Include(r => r.Department)
            .Include(r => r.assignTo)
            .Include(r => r.SpecialFieldItems)
            .Include(r => r.SpecialFieldListItems)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return Error.NotFound("Request.NotFound", _localizer[SharedResourcesKeys.PurchaseRequestNotFound]);

        if (request.Department == null || !request.Department.RequiresGoodsReceipt)
            return Error.Validation("Request.NotProcurement", "Goods receipts are only supported for procurement department requests.");

        if (!request.NeedsReceiptConfirmation)
        {
            return IsPurchaseClosed(request.Status)
                ? Error.Conflict("Request.AlreadyClosed", _localizer[SharedResourcesKeys.PurchaseRequestAlreadyClosed])
                : Error.Conflict("Request.NotReadyForReceipt", _localizer[SharedResourcesKeys.PurchaseRequestNotReadyForReceipt]);
        }

        var engineer = await _db.Engineers
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ApplicationUserId == Guid.Parse(CurrentUser.UserId));

        var receipt = new PurchaseRequestReceipt
        {
            EngineerRequestId = request.Id,
            ReceivedById = engineer?.Id,
            ReceiptDate = dto.ReceiptDate ?? DateTimeHelper.Now.DateTime,
            IsPartialReceipt = dto.IsPartialReceipt,
            IsConfirmed = dto.IsConfirmed,
            Notes = dto.Notes,
        };

        await _db.PurchaseRequestReceipts.AddAsync(receipt);

        Guid newStatusId = request.StatusId;
        string actionType;

        if (dto.IsConfirmed)
        {
            // Full or partial receipt confirmed → close the request
            var closedStatus = await GetPurchaseClosedStatusAsync();
            if (closedStatus != null)
            {
                newStatusId = closedStatus.Id;
                request.StatusId = closedStatus.Id;
            }
            request.NeedsReceiptConfirmation = false;
            actionType = EngineerRequestActionType.ClosedOnReceipt.ToString();
        }
        else if (dto.IsPartialReceipt)
        {
            // Partial receipt not yet confirmed → return to office engineer (in-progress)
            var statuses = await _db.Statuses
                .AsNoTracking()
                .Select(s => new StatusKeywordProjection(s.Id, s.nameEn, s.nameAr, s.Code))
                .ToListAsync();
            var inProgressStatusId = FindStatusIdByKeywords(statuses, InProgressKeywords);
            if (inProgressStatusId.HasValue)
            {
                newStatusId = inProgressStatusId.Value;
                request.StatusId = inProgressStatusId.Value;
            }
            actionType = EngineerRequestActionType.PartialReceiptPendingReview.ToString();
        }
        else
        {
            actionType = EngineerRequestActionType.GoodsReceiptRecorded.ToString();
        }

        await _db.EngineerRequestActivites.AddAsync(new EngineerRequestActivite
        {
            EngineerRequestId = request.Id,
            EngineerId = engineer?.Id,
            StatusId = newStatusId,
            ActionType = actionType,
            Comments = dto.Notes,
        });

        // Update received quantities on items
        if (request.SpecialFieldItems != null && request.SpecialFieldItems.Any())
        {
            if (dto.IsPartialReceipt && dto.SpecialFieldItems != null && dto.SpecialFieldItems.Any())
            {
                // Partial receipt: apply the quantities provided by the engineer.
                // Every submitted ItemId must resolve to a real row on this request (by row Id or,
                // when unambiguous, by ConstructionItemId) — otherwise we'd silently persist nothing
                // for that line while still returning 200 OK.
                var unresolved = ApplySpecialFieldItemReceivedQuantities(request.SpecialFieldItems, dto.SpecialFieldItems);
                if (unresolved.Count > 0)
                {
                    return Error.Validation(
                        "SpecialFieldItem.NotFound",
                        $"Item(s) {string.Join(", ", unresolved)} were not found on this request.");
                }
            }
            else if (!dto.IsPartialReceipt)
            {
                // Full receipt: mark all items as fully received
                foreach (var item in request.SpecialFieldItems)
                    item.ReceivedQuantity = item.Quantity;
            }
        }

        // Update received quantities on generic list-group items (e.g. Procurement's
        // "Qty Required" rows configured via ListGroupKey instead of ConstructionItem).
        if (request.SpecialFieldListItems != null && request.SpecialFieldListItems.Any())
        {
            if (dto.IsPartialReceipt && dto.SpecialFieldListItems != null && dto.SpecialFieldListItems.Any())
            {
                var unresolvedListItems = ApplySpecialFieldListItemReceivedQuantities(request.SpecialFieldListItems, dto.SpecialFieldListItems);
                if (unresolvedListItems.Count > 0)
                {
                    return Error.Validation(
                        "SpecialFieldListItem.NotFound",
                        $"Item(s) {string.Join(", ", unresolvedListItems)} were not found on this request.");
                }
            }
            else if (!dto.IsPartialReceipt)
            {
                // Full receipt: for any list-item cell whose value parses as a quantity, mark
                // it fully received. Non-numeric cells (S/N, description, unit, etc.) are untouched.
                foreach (var item in request.SpecialFieldListItems)
                {
                    if (int.TryParse(item.value, out var qty))
                        item.ReceivedQuantity = qty;
                }
            }
        }

        await _db.SaveChangesAsync();

        // Notify assigned office engineer on partial receipt (not confirmed) — needs review
        if (dto.IsPartialReceipt && !dto.IsConfirmed && request.assignToId.HasValue)
        {
            var assignedEngineer = request.assignTo
                ?? await _db.Engineers.FirstOrDefaultAsync(e => e.Id == request.assignToId.Value);

            if (assignedEngineer?.ApplicationUserId is not null && assignedEngineer.ApplicationUserId != Guid.Empty)
            {
                await _notificationService.SendNotificationToUserAsync(
                    assignedEngineer.ApplicationUserId,
                    _localizer[SharedResourcesKeys.NotificationPartialReceiptTitle],
                    _localizer[SharedResourcesKeys.NotificationPartialReceiptBody],
                    request.Id);
            }
        }

        // Notify assigned office engineer when receipt is confirmed (request closed)
        if (dto.IsConfirmed && request.assignToId.HasValue)
        {
            var assignedEngineer = request.assignTo
                ?? await _db.Engineers.FirstOrDefaultAsync(e => e.Id == request.assignToId.Value);

            if (assignedEngineer?.ApplicationUserId is not null && assignedEngineer.ApplicationUserId != Guid.Empty)
            {
                await _notificationService.SendNotificationToUserAsync(
                    assignedEngineer.ApplicationUserId,
                    _localizer[SharedResourcesKeys.NotificationReceiptConfirmedTitle],
                    _localizer[SharedResourcesKeys.NotificationReceiptConfirmedBody],
                    request.Id);
            }
        }

        return await GetEngineerRequestByIdAsync(requestId);
    }

    public async Task<ErrorOr<List<GetGoodsReceiptDto>>> GetGoodsReceiptsAsync(Guid requestId)
    {
        var request = await _db.EngineerRequests
            .AsNoTracking()
            .Include(r => r.Department)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request is null)
            return Error.NotFound("Request.NotFound", _localizer[SharedResourcesKeys.PurchaseRequestNotFound]);

        if (request.Department == null || !request.Department.RequiresGoodsReceipt)
            return Error.Validation("Request.NotProcurement", "Goods receipts are only supported for procurement department requests.");

        var receipts = await _db.PurchaseRequestReceipts
            .AsNoTracking()
            .Where(r => r.EngineerRequestId == requestId)
            .Include(r => r.ReceivedBy)
            .OrderByDescending(r => r.ReceiptDate)
            .ToListAsync();

        return receipts.Select(MapReceiptToDto).ToList();
    }

    private static bool IsPurchaseClosed(Status? status)
        => status != null && PurchaseClosedKeywords.Any(k =>
            (status.Code?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameEn?.Contains(k, StringComparison.OrdinalIgnoreCase) == true) ||
            (status.nameAr?.Contains(k, StringComparison.OrdinalIgnoreCase) == true));

    private async Task<Status?> GetPurchaseClosedStatusAsync()
    {
        var statuses = await _db.Statuses.AsNoTracking().ToListAsync();
        return statuses.FirstOrDefault(s => IsPurchaseClosed(s))
               ?? statuses.OrderByDescending(s => s.orderNumber).FirstOrDefault();
    }

    private GetGoodsReceiptDto MapReceiptToDto(PurchaseRequestReceipt rc)
        => new()
        {
            Id = rc.Id,
            ReceiptDate = rc.ReceiptDate,
            IsPartialReceipt = rc.IsPartialReceipt,
            IsConfirmed = rc.IsConfirmed,
            Notes = rc.Notes,
            ReceivedById = rc.ReceivedById,
            ReceivedBy = rc.ReceivedBy is null ? null : _mapper.Map<Contracting.Shared.Dtos.MasterDtos.EngineerDto.GetEngineerDto>(rc.ReceivedBy),
        };

    // Validates that any SpecialFieldValue whose fieldType is "ConstructionItem" contains a valid, existing ConstructionItem ID.
    private async Task<Error?> ValidateConstructionItemFieldsAsync(
        List<CreateEngineerRequestSpecialFieldValueDto>? sfvDtos,
        CancellationToken cancellationToken = default)
    {
        if (sfvDtos == null || !sfvDtos.Any()) return null;

        var fieldIds = sfvDtos.Select(s => s.DepartmentSpecialFieldId).ToList();
        var departmentSpecialFields = await _db.DepartmentSpecialFields
            .Include(dsf => dsf.SpecialField)
            .Where(dsf => fieldIds.Contains(dsf.Id))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var fieldLookup = departmentSpecialFields.ToDictionary(dsf => dsf.Id);
        var constructionItemIds = new List<Guid>();

        foreach (var sfv in sfvDtos)
        {
            if (!fieldLookup.TryGetValue(sfv.DepartmentSpecialFieldId, out var dsf)) continue;
            if (dsf.SpecialField == null) continue;
            if (!string.Equals(dsf.SpecialField.fieldType, "ConstructionItem", StringComparison.OrdinalIgnoreCase)) continue;

            if (!Guid.TryParse(sfv.value, out var itemId))
                return Error.Validation(
                    "SpecialField.InvalidConstructionItem",
                    _localizer[SharedResourcesKeys.InvalidConstructionItemField]);

            constructionItemIds.Add(itemId);
        }

        if (!constructionItemIds.Any()) return null;

        var existingIds = await _db.ConstructionItems
            .Where(ci => constructionItemIds.Contains(ci.Id))
            .Select(ci => ci.Id)
            .ToListAsync(cancellationToken);

        var missingId = constructionItemIds.Except(existingIds).FirstOrDefault();
        if (missingId != Guid.Empty)
            return Error.NotFound(
                "ConstructionItem.NotFound",
                _localizer[SharedResourcesKeys.ConstructionItemNotFound]);

        return null;
    }
}
