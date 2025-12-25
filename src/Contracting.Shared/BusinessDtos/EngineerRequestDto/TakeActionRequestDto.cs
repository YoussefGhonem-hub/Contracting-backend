namespace Contracting.Shared.BusinessDtos.EngineerRequestDto
{
    public class TakeActionRequestDto
    {
        public Guid? projectId { get; set; }
        public Guid? statusId { get; set; }
        public Guid? assignToId { get; set; }
        public bool isAprroved { get; set; }
        public string? note { get; set; }

    }
}
