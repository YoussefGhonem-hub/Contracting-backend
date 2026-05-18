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
        public ICollection<DepartmentSpecialField> DepartmentSpecialFields { get; set; } = new List<DepartmentSpecialField>();
        public ICollection<Engineer> Engineers { get; set; }
        public ICollection<EngineerDepartment> EngineerDepartments { get; set; } = new List<EngineerDepartment>();

    }
}
