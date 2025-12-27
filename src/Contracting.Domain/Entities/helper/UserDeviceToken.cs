using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.helper
{
    public class UserDeviceToken : BaseAuditableEntity
    {
        public Guid UserId { get; set; }
        public string FcmToken { get; set; } = default!;
    }
}
