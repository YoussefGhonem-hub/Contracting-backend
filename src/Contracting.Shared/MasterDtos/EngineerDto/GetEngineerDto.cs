using Contracting.Shared.MasterDtos.DepartmentDtos;

namespace Contracting.Shared.MasterDtos.EngineerDto
{
    public class GetEngineerDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? passportNumber { get; set; }
        public string? nationalId { get; set; }
        public string? position { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
    }

    public class GetEngineerDropDownDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }      
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
    }
}
