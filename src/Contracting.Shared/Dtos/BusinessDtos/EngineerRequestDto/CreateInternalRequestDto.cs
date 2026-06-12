using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;
using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class CreateInternalRequestDto
    {
        /// <summary>Department within the requester's branch to route the request to.</summary>
        public Guid DepartmentId { get; set; }

        /// <summary>The engineer in that department who will receive the request directly.</summary>
        public Guid AssignToEngineerId { get; set; }

        public Guid? PriorityId { get; set; }
        public string? RequestTitle { get; set; }
        public string? Descreption { get; set; }
        public string? Notes { get; set; }
        public ICollection<IFormFile>? Attachments { get; set; }
    }
}
