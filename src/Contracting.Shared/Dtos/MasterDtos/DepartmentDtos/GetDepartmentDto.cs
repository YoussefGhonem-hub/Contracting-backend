namespace Contracting.Shared.Dtos.MasterDtos.DepartmentDtos
{
    public class GetDepartmentDto
    {
        public Guid Id { get; set; }
        public string? nameEn { get; set; }
        public string? nameAr { get; set; }
        public Guid? BranchId { get; set; }
        public Contracting.Shared.Dtos.MasterDtos.BranchDto.BranchDropDownDto? Branch { get; set; }
    }
}
