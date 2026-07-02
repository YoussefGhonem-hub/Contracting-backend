namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class CreateFinancialClearanceItemDto
    {
        public string ItemName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
