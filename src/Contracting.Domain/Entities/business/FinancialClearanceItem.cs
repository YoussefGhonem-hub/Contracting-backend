using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class FinancialClearanceItem : BaseEntity
    {
        public Guid FinancialClearanceId { get; set; }
        public FinancialClearance? FinancialClearance { get; set; }
        public string? Code { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Value { get; set; }
    }
}
