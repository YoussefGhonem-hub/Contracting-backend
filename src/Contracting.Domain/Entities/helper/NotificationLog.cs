using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.helper
{
    public class NotificationLog : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public string? Token { get; set; }
        public string Title { get; set; } = default!;
        public string Body { get; set; } = default!;
        public bool IsSent { get; set; }
        public string? ErrorMessage { get; set; }
        public Guid? EngineerId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? RequestId { get; set; }
        public DateTimeOffset SentAt { get; set; }
    }
}
