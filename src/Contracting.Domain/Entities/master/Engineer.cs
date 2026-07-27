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
        public int? yearExperience { get; set; }
        public string? phoneNumber { get; set; }
        public string? Email { get; set; }
        // When set, performance/site-analytics calculates this engineer's metrics starting from
        // this date instead of CreatedDate. Null falls back to CreatedDate.
        public DateTime? EffectiveDate { get; set; }
        public Guid ApplicationUserId { get; set; }
        public ApplicationUser ApplicationUser { get; set; }
        public Guid? DepartmentId { get; set; }
        public Department? Department { get; set; }
        public ICollection<EngineerProject> EngineerProjects { get; set; } = new List<EngineerProject>();
        public ICollection<EngineerDepartment> EngineerDepartments { get; set; } = new List<EngineerDepartment>();
    }
}
