namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class FinancialClearanceActionDto
    {
        /// <summary>Assign | Submit | Review | Approve | Close | Reject | MissingInfo</summary>
        public string ActionType { get; set; } = string.Empty;
        public string? Comments { get; set; }
        public Guid? AssignedToId { get; set; }
    }
}
