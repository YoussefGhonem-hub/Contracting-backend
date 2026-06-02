using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class UpdateFinancialClearanceDto
    {
        public Guid Id { get; set; }
        public string? EmployeeName { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? ProjectId { get; set; }
        public DateTime? RequestDate { get; set; }
        public decimal? AdvanceAmount { get; set; }
        public decimal? SpentAmount { get; set; }
        public string? Notes { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
        public List<string>? AttachmentTypes { get; set; }
    }
}
