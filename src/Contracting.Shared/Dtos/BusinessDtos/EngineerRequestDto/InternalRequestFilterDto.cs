namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class InternalRequestFilterDto : Contracting.Shared.Dtos.BaseFilterDto
    {
        public Guid? DepartmentId { get; set; }
        public Guid? BranchId { get; set; }
        public Guid? StatusId { get; set; }
        public Guid? PriorityId { get; set; }
        /// <summary>Office engineer who created the request.</summary>
        public Guid? RequestedById { get; set; }
        /// <summary>Engineer the request is assigned to.</summary>
        public Guid? AssignedToId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? Search { get; set; }
    }
}
