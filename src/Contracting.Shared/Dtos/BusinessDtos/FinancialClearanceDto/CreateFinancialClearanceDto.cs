using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class CreateFinancialClearanceDto
    {
        public string? EmployeeName { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTime RequestDate { get; set; } = DateTime.UtcNow;
        // Clearance period — both required; ToDate must be on or after FromDate.
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal AdvanceAmount { get; set; }
        public List<CreateFinancialClearanceItemDto> Items { get; set; } = new();
        public string? Notes { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
        /// <summary>Parallel list of attachment types: Invoice, Receipt, Supporting</summary>
        public List<string>? AttachmentTypes { get; set; }
    }
}
