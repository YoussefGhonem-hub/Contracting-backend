namespace Contracting.Shared.BusinessDtos.EngineerRequestActiviteDto
{
    public class GetEngineerRequestActiviteDto
    {
        public Guid Id { get; set; }
        public Guid? EngineerRequestId { get; set; }
        public Guid? EngineerId { get; set; }
        public string? EngineerName { get; set; }
        public Guid? StatusId { get; set; }
        public string? StatusName { get; set; }
        public string? ActionType { get; set; }
        public string? Comments { get; set; }
        public DateTimeOffset CreatedDate { get; set; }
    }
}
