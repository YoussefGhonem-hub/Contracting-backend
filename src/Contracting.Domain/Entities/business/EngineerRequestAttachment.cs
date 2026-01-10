using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequestAttachment : BaseAuditableEntity
    {
        public string? Key { get; set; }
        public string? FileName { get; set; }
        public string? Extension { get; set; }
        public long? FileSize { get; set; }
        public string? Url { get; set; }

        // Optional relationships
        public Guid? EngineerRequestId { get; set; }
        public EngineerRequest? EngineerRequest { get; set; }

        public Guid? EngineerRequestNotesId { get; set; }
        public EngineerRequestNotes? EngineerRequestNotes { get; set; }
    }
}
