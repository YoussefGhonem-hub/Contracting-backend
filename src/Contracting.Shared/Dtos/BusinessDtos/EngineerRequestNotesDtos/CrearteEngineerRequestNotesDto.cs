using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos
{
    public class CrearteEngineerRequestNotesDto
    {
        public string? note { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }

    public class UpdateEngineerRequestNotesDto
    {
        public Guid? Id { get; set; }
        public string? note { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}
