using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class TransferRequestActivity : BaseAuditableEntity
    {
        public Guid TransferRequestId { get; set; }
        public TransferRequest? TransferRequest { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public Guid? FromStatusId { get; set; }
        public Status? FromStatus { get; set; }
        public Guid ToStatusId { get; set; }
        public Status? ToStatus { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
    }
}
