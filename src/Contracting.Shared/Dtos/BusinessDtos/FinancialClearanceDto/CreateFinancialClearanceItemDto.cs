namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class CreateFinancialClearanceItemDto
    {
        public string? Code { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Value { get; set; }
    }
}
