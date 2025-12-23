namespace Contracting.Shared.MasterDtos.EngineerDto
{
    public class CreateEngineerDto
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
        public string? password { get; set; }
        public bool isManager { get; set; } = false;

        public Guid DepartmentId { get; set; }
    }
}
