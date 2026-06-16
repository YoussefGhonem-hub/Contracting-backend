using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using Contracting.Shared.Dtos.MasterDtos.ProjectDtos;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class GetFinancialClearanceDto
    {
        public Guid Id { get; set; }
        public string? ClearanceNumber { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? Notes { get; set; }
        public Guid? StatusId { get; set; }
        public GetDropDownStatusDto? Status { get; set; }
        public Guid? RequestedById { get; set; }
        public GetEngineerDto? RequestedBy { get; set; }
        public Guid? AssignedToId { get; set; }
        public GetEngineerDto? AssignedTo { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
        public List<GetFinancialClearanceAttachmentDto> Attachments { get; set; } = new();
        public List<GetFinancialClearanceActivityDto> Activities { get; set; } = new();
    }
}
