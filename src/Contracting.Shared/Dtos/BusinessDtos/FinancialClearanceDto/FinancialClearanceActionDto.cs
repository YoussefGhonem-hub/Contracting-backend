using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class FinancialClearanceActionDto
    {
        /// <summary>Assign | Submit | Review | Approve | Close | Reject | MissingInfo</summary>
        public string ActionType { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public Guid? AssignedToId { get; set; }
        /// <summary>Optional file to attach alongside this action (e.g. receipt when closing).</summary>
        public IFormFile? Attachment { get; set; }
        /// <summary>Invoice | Receipt | Supporting — optional label for the attached file.</summary>
        public string? AttachmentType { get; set; }
    }
}
