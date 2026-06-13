using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;

namespace Contracting.Shared.Dtos.MasterDtos.BranchDto
{
    public class UpdateBranchDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public string? address { get; set; }
        public string? location { get; set; }
        public string? currency { get; set; }
        public List<UpdateDepartmentDto> Departments { get; set; } = new();
    }
}
