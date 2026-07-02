using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class FinancialClearance : BaseAuditableEntity
    {
        public string? ClearanceNumber { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
        public DateTime RequestDate { get; set; }
        public decimal AdvanceAmount { get; set; }
        public decimal SpentAmount { get; set; }
        public decimal RemainingAmount { get; set; }
        public string? Notes { get; set; }
        public Guid? StatusId { get; set; }
        public Status? Status { get; set; }
        public Guid? RequestedById { get; set; }
        public Engineer? RequestedBy { get; set; }
        // Engineer the request is assigned to (set by the team lead via assign/reassign).
        public Guid? AssignedToId { get; set; }
        public Engineer? AssignedTo { get; set; }

        public ICollection<FinancialClearanceItem> Items { get; set; } = new List<FinancialClearanceItem>();
        public ICollection<FinancialClearanceAttachment> Attachments { get; set; } = new List<FinancialClearanceAttachment>();
        public ICollection<FinancialClearanceActivity> Activities { get; set; } = new List<FinancialClearanceActivity>();
    }
}
