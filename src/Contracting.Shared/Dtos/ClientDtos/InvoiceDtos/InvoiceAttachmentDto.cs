namespace Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;

public class InvoiceAttachmentDto
{
    public Guid Id { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
