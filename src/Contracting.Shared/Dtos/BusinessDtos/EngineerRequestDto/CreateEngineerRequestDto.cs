using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateEngineerRequestDto
    {
        public Guid? ProjectId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PriorityId { get; set; }
        public string? RequestTitle { get; set; }
        public string? Descreption { get; set; }
        public ICollection<CrearteEngineerRequestNotesDto> EngineerRequestNotes { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}

