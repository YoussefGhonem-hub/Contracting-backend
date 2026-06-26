using Contracting.Domain.Common;
using Contracting.Domain.Common.Enums;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.helper
{
    public class NotificationLog : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public string? Token { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public bool IsSent { get; set; }
        public bool IsRead { get; set; }
        public DateTimeOffset? ReadAt { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RequestId { get; set; }
        public NotificationKey? Key { get; set; }
        public DateTimeOffset SentAt { get; set; }
    }
}
