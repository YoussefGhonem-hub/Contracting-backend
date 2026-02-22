using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class SpecialField : BaseAuditableEntity
    {
        public string? name { get; set; }
        public string? fieldType { get; set; }

        public ICollection<DepartmentSpecialField> DepartmentSpecialFields { get; set; } = new List<DepartmentSpecialField>();
    }
}