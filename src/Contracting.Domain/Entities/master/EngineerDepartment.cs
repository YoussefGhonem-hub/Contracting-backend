using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class EngineerDepartment : BaseAuditableEntity
    {
        public Guid EngineerId { get; set; }
        public Engineer? Engineer { get; set; }

        public Guid DepartmentId { get; set; }
        public Department? Department { get; set; }

        public Guid RoleId { get; set; }
        public ApplicationRole? Role { get; set; }
    }
}
