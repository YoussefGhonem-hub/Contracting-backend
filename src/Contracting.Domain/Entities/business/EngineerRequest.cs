using Contracting.Domain.Common;
using Contracting.Domain.Entities.master;

namespace Contracting.Domain.Entities.business
{
    public class EngineerRequest : BaseAuditableEntity
    {
        public Guid? ProjectId { get; set; }
        public Project? Project { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public Guid? PriorityId { get; set; }
        public Priority? Priority { get; set; }
        public Guid? EngineerId { get; set; }
        public Engineer? Engineer { get; set; }
        public string? Descreption { get; set; }
        public int timeDuration { get; set; }
        public Guid StatusId { get; set; }
        public Status Status { get; set; }
        public Guid? assignToId { get; set; }
        public Engineer? assignTo { get; set; }
        public string? Note { get; set; }
        public DateTime? NoteDate { get; set; }
        public ICollection<EngineerRequestNotes> EngineerRequestNotes { get; set; }
    }
}
