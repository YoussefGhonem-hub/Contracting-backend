using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class UpdateEngineerRequestDto
    {
        public Guid Id { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? DepartmentId { get; set; }
        public Guid? PriorityId { get; set; }
        public string? RequestTitle { get; set; }
        public string? Descreption { get; set; }
        public ICollection<UpdateEngineerRequestNotesDto>? EngineerRequestNotes { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
        public List<CreateEngineerRequestSpecialFieldValueDto>? SpecialFieldValues { get; set; } = new();
        public List<CreateEngineerRequestSpecialFieldItemDto>? SpecialFieldItems { get; set; } = new();
        public List<CreateEngineerRequestSpecialFieldListItemDto>? SpecialFieldListItems { get; set; }

    }
}
