using Contracting.Domain.Common;
using Contracting.Domain.Entities.business.enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class TransferRequestActivity : BaseAuditableEntity
    {
        public Guid TransferRequestId { get; set; }
        public TransferRequest? TransferRequest { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public TransferRequestStatus? FromStatus { get; set; }
        public TransferRequestStatus ToStatus { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
    }
}
