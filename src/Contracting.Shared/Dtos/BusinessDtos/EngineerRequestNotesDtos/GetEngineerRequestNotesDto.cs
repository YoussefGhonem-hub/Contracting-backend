using Contracting.Shared.Dtos.MasterDtos.EngineerDto;

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
