namespace Contracting.Shared.Dtos.ClientDtos.DrawingDtos;

public class GetClientDrawingDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? Type { get; set; }
    public string? FileName { get; set; }
    public string? Extension { get; set; }
    public long? FileSize { get; set; }
    public string? Url { get; set; }
    public DateTimeOffset UploadedAt { get; set; }
}
