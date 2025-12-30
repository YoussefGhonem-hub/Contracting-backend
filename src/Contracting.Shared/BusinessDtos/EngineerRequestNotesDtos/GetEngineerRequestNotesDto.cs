using Contracting.Shared.MasterDtos.EngineerDto;
using Contracting.Shared.MasterDtos.StatusDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos
{
    public class GetEngineerRequestNotesDto
    {
        public Guid Id { get; set; }
        public string? note { get; set; }
        public Guid? EngineerId { get; set; }
        public GetEngineerDto? Engineer { get; set; }
        public DateTimeOffset CreatedDate { get; set; }

    }
}
