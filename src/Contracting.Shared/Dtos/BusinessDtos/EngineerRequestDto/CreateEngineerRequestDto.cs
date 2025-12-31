using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateEngineerRequestDto
    {
        public Guid? ProjectId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PriorityId { get; set; }
        public string? Descreption { get; set; }
        public ICollection<CrearteEngineerRequestNotesDto> EngineerRequestNotes { get; set; }
    }
}

