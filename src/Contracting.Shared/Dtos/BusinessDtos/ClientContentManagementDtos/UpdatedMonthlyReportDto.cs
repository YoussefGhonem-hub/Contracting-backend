namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdatedMonthlyReportDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string? Title { get; set; }
    public string? WorkProgress { get; set; }
    public List<MonthlyReportAttachmentDto> Attachments { get; set; } = new();
}

public class MonthlyReportAttachmentDto
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
    public long? FileSize { get; set; }
}
