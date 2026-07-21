using System;
using Contracting.Shared.Dtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class EngineerRequestParticipationFilterDto : BaseFilterDto
    {
        public Guid? BranchId { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AssignToId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? StatusId { get; set; }
        /// <summary>Filter by request type: EngineerRequest, InternalRequest, TransferRequest, LaborAttendance, FinancialClearance</summary>
        public string? RequestType { get; set; }
    }
}
