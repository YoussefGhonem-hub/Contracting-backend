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
        public string? Note { get; set; }
        public DateTime? NoteDate { get; set; }
    }
}
