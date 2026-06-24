namespace Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;

public class GetClientVariationOrderDetailDto
{
    public Guid Id { get; set; }
    public int? VONumber { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public DateTimeOffset? ClientActionDate { get; set; }
    public string? ClientRejectionReason { get; set; }
    public List<VOAttachmentDto> Attachments { get; set; } = new();
}

public class VOAttachmentDto
{
    public Guid Id { get; set; }
    public string? Key { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
