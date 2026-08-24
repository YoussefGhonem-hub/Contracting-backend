namespace Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos
{
    public class GetRequestTypeDefaultDepartmentDto
    {
        public Guid Id { get; set; }
        public Guid BranchId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
        public string? DepartmentNameEn { get; set; }
        public string? DepartmentNameAr { get; set; }
    }
}
