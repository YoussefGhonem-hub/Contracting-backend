using Contracting.Shared.MasterDtos.DepartmentDtos;
using Contracting.Shared.MasterDtos.EngineerDto;
using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.ProjectDtos;
using Contracting.Shared.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class GetAllEngineerRequestDto
    {
        public Guid? ProjectId { get; set; }
        public GetProjectDto? Project { get; set; }
        public Guid? DepartmentId { get; set; }
        public GetDepartmentDto? Department { get; set; }
        public Guid? PriorityId { get; set; }
        public GetDropDownPriorityDto? Priority { get; set; }
        public Guid? EngineerId { get; set; }
        public GetEngineerDto? Engineer { get; set; }
        public string? Descreption { get; set; }
        public string? Note { get; set; }
        public DateTime? NoteDate { get; set; }
    }
}
