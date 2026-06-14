using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.LaborAttendanceDto
{
    public class GetLaborAttendanceRequestDto
    {
        public Guid Id { get; set; }
        public string? RequestNumber { get; set; }
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public string? SiteName { get; set; }
        public DateTime AttendanceDate { get; set; }
        public Guid? SupervisorId { get; set; }
        public GetEngineerDto? Supervisor { get; set; }
        public Guid? AssignedToId { get; set; }
        public GetEngineerDto? AssignedTo { get; set; }
        public string? Notes { get; set; }
        public Guid? StatusId { get; set; }
        public GetDropDownStatusDto? Status { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public List<GetLaborAttendanceRecordDto> Records { get; set; } = new();
        public List<GetAttachmentDto> Attachments { get; set; } = new();
        public List<GetLaborAttendanceActivityDto> Activities { get; set; } = new();
    }
}
