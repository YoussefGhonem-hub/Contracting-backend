namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class GetFinancialClearanceItemDto
    {
        public Guid Id { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}
