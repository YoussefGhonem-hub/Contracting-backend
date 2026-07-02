namespace Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;

public class GetClientInvoiceListItemDto
{
    public Guid Id { get; set; }
    public int? InvoiceNumber { get; set; }
    public string? Title { get; set; }
    public decimal TotalValue { get; set; }
    public decimal PaidAmount { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public List<InvoiceAttachmentDto> Attachments { get; set; } = new();
}
