using System;
using Contracting.Shared.Dtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class EngineerRequestParticipationFilterDto : BaseFilterDto
    {
        public Guid? ProjectId { get; set; }
        public Guid? AssignToId { get; set; }
        public Guid? DepartmentId { get; set; }
    }
}
