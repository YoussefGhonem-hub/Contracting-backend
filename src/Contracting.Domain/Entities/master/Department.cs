using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Department :BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public Guid BranchId { get; set; }
        public Branch Branch { get; set; }
        public bool hasSpecialFields { get; set; }
        // When true: office engineer "confirm" triggers site engineer receipt confirmation before closing
        public bool RequiresGoodsReceipt { get; set; }
        // When true: site engineer who created the transfer request is notified when it is completed
        public bool NotifyOnTransferComplete { get; set; }
        // When true: department members are notified when a labor attendance request is cost-approved
        public bool NotifyAfterLaborApprove { get; set; }
        public ICollection<DepartmentSpecialField> DepartmentSpecialFields { get; set; } = new List<DepartmentSpecialField>();
        public ICollection<Engineer> Engineers { get; set; }
        public ICollection<EngineerDepartment> EngineerDepartments { get; set; } = new List<EngineerDepartment>();

    }
}
