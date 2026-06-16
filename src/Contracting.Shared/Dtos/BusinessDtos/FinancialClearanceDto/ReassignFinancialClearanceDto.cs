namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    /// <summary>Reassigns a financial clearance request to a different engineer (status unchanged).</summary>
    public class ReassignFinancialClearanceDto
    {
        public Guid AssignedToId { get; set; }
        public string? Comments { get; set; }
    }
}
