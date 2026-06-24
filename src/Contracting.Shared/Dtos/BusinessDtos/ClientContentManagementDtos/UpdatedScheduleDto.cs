namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdatedScheduleDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? Title { get; set; }
    public string? Version { get; set; }
    public string? Url { get; set; }
    public string? FileName { get; set; }
}
