namespace Contracting.Shared.Dtos.ClientDtos.ReportDtos;

public class GetClientSiteReportListItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
}
