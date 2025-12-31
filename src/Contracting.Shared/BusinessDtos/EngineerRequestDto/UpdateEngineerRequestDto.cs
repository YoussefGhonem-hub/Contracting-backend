using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class UpdateEngineerRequestDto
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PriorityId { get; set; }
        public string? Descreption { get; set; }
        public ICollection<UpdateEngineerRequestNotesDto> EngineerRequestNotes { get; set; }

    }
}
