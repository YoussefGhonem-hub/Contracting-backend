namespace Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos
{
    public class CreateRequestTypeDefaultDepartmentDto
    {
        public Guid BranchId { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public Guid DepartmentId { get; set; }
    }
}
