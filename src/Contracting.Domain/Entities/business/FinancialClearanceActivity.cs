using Contracting.Domain.Common;
using Contracting.Domain.Entities.business.enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class FinancialClearanceActivity : BaseAuditableEntity
    {
        public Guid FinancialClearanceId { get; set; }
        public FinancialClearance? FinancialClearance { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public FinancialClearanceStatus? FromStatus { get; set; }
        public FinancialClearanceStatus ToStatus { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
    }
}
