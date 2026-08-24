using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    /// <summary>
    /// Per-branch configuration of which department a given request type should default to
    /// (e.g. FinancialClearance -> "Cost control", LaborAttendance -> some other department).
    /// The create-request page calls the lookup for its request type and pre-fills the department;
    /// if none is configured, the engineer picks it manually.
    /// </summary>
    public class RequestTypeDefaultDepartment : BaseAuditableEntity
    {
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; } = null!;
        public string RequestType { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public Department Department { get; set; } = null!;
    }
}
