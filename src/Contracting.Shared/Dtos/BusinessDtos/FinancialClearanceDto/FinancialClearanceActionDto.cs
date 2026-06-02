namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class FinancialClearanceActionDto
    {
        /// <summary>Submit, Review, Approve, Close, Reject</summary>
        public string ActionType { get; set; } = string.Empty;
        public string? Comments { get; set; }
    }
}
