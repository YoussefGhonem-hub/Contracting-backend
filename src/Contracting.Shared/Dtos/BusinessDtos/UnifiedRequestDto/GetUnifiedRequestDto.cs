using Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Contracting.Shared.BusinessDtos.TransferRequestDto;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using Contracting.Shared.BusinessDtos.FinancialClearanceDto;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.PriorityDto;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.UnifiedRequestDto
{
    /// <summary>
    /// Unified DTO representing all types of requests in the system
    /// </summary>
    public class GetUnifiedRequestDto
    {
        public Guid Id { get; set; }
        public string? RequestNumber { get; set; }
        public string RequestType { get; set; } = string.Empty; // "EngineerRequest", "TransferRequest", "LaborAttendance", "FinancialClearance"
        
        // Common fields
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }
        public Guid? RequestedById { get; set; }
        public GetEngineerDto? RequestedBy { get; set; }
        public Guid? StatusId { get; set; }
        public GetDropDownStatusDto? Status { get; set; }
        public string? Notes { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        
        // Engineer Request specific fields
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public Guid? PriorityId { get; set; }
        public GetDropDownPriorityDto? Priority { get; set; }
        public string? RequestTitle { get; set; }
        public string? Description { get; set; }
        public Guid? AssignedToId { get; set; }
        public GetEngineerDto? AssignedTo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool NeedsReceiptConfirmation { get; set; }
        
        // Transfer Request specific fields
        public Guid? SourceProjectId { get; set; }
        public GetProjectDto? SourceProject { get; set; }
        public string? SourceWarehouse { get; set; }
        public Guid? DestinationProjectId { get; set; }
        public GetProjectDto? DestinationProject { get; set; }
        public string? DestinationWarehouse { get; set; }
        public List<GetTransferRequestItemDto> TransferItems { get; set; } = new();
        
        // Labor Attendance specific fields
        public string? SiteName { get; set; }
        public DateTime? AttendanceDate { get; set; }
        public Guid? SupervisorId { get; set; }
        public GetEngineerDto? Supervisor { get; set; }
        public decimal? TotalAmount { get; set; }
        public List<GetLaborAttendanceRecordDto> LaborRecords { get; set; } = new(); // Worker/Labor info for finance review
        
        // Financial Clearance specific fields
        public string? ClearanceNumber { get; set; }
        public string? EmployeeName { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? SpentAmount { get; set; }
        public decimal? RemainingAmount { get; set; }
        
        // Common collections
        public List<GetAttachmentDto> EngineerRequestAttachments { get; set; } = new();

        // Engineer Request special fields
        public List<EngineerRequestSpecialFieldValueDto> SpecialFieldValues { get; set; } = new();
        public List<GetEngineerRequestSpecialFieldItemDto> SpecialFieldItems { get; set; } = new();

        // Engineer Request notes and activities
        public List<GetEngineerRequestNotesDto> EngineerRequestNotes { get; set; } = new();
        public List<GetEngineerRequestActiviteDto> EngineerRequestActivites { get; set; } = new();

        // Original request data (for detailed view)
        public object? OriginalRequest { get; set; }
    }
}
