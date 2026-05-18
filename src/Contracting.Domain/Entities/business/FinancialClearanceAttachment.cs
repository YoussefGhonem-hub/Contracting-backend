using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class FinancialClearanceAttachment : BaseAuditableEntity
    {
        public Guid FinancialClearanceId { get; set; }
        public FinancialClearance? FinancialClearance { get; set; }
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? FileSize { get; set; }
        public string? Url { get; set; }
        /// <summary>Invoice, Receipt, Supporting</summary>
        public string? AttachmentType { get; set; }
    }
}
