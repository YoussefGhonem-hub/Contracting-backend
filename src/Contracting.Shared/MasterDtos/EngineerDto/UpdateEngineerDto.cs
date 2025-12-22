namespace Contracting.Shared.MasterDtos.EngineerDto
{
    public class UpdateEngineerDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? passportNumber { get; set; }
        public string? nationalId { get; set; }
        public string? position { get; set; }
        public Guid? ChangeDepartmentId { get; set; }
    }
}
