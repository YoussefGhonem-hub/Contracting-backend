using Contracting.Shared.BusinessDtos.EngineerRequestNotesDtos;

namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class TakeActionRequestDto
    {
        public Guid? statusId { get; set; }
        public Guid? assignToId { get; set; }
        public int? timeDuration { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public ICollection<CrearteEngineerRequestNotesDto>? EngineerRequestNotes { get; set; }
        public List<EngineerRequestSpecialFieldItemReceiptDto> SpecialFieldItems { get; set; } = new();
    }

    public class EngineerRequestSpecialFieldItemReceiptDto
    {
        public Guid ItemId { get; set; }
        public int ReceivedQuantity { get; set; }
    }
}
