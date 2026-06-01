namespace Contracting.Shared.Dtos.ClientDtos.ReportDtos;

public class GetClientSiteReportDetailDto
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string? Title { get; set; }
    public string? WorkProgress { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public DateTimeOffset CreatedDate { get; set; }
    public List<ClientSiteReportAttachmentDto> Attachments { get; set; } = new();
}

public class ClientSiteReportAttachmentDto
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
