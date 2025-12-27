using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class TakeActionRequestDto
    {
        public Guid? projectId { get; set; }
        public Guid? statusId { get; set; }
        public Guid? assignToId { get; set; }
        public int? timeDuration { get; set; }
        public bool isAprroved { get; set; }
        public ICollection<CrearteEngineerRequestNotesDto> EngineerRequestNotes { get; set; }

    }
}
