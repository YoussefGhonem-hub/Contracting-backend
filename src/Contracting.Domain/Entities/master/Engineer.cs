using Contracting.Domain.Common;

namespace Contracting.Domain.Entities.master
{
    public class Engineer : BaseAuditableEntity
    {
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? passportNumber { get; set; }
        public string? nationalId { get; set; }
        public string? position { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
    }
}
