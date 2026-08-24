namespace Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos
{
    /// <summary>
    /// Response for the create-request-page lookup. HasDefault is false when no default is
    /// configured for this branch/request-type combination — the client should fall back to
    /// letting the engineer pick the department manually.
    /// </summary>
    public class GetDefaultDepartmentDto
    {
        public bool HasDefault { get; set; }
        public Guid? DepartmentId { get; set; }
        public string? DepartmentNameEn { get; set; }
        public string? DepartmentNameAr { get; set; }
    }
}
