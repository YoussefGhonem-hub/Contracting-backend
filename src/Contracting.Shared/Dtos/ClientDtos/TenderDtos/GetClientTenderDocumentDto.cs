namespace Contracting.Shared.Dtos.ClientDtos.TenderDtos;

public class GetClientTenderDocumentDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
}
