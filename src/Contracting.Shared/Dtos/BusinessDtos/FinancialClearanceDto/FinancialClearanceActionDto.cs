using Contracting.Shared.Constants;

namespace Contracting.Shared.BusinessDtos.FinancialClearanceDto
{
    public class FinancialClearanceActionDto
    {
        public FinancialClearanceActionType ActionType { get; set; }
        public string? Comments { get; set; }
    }
}
