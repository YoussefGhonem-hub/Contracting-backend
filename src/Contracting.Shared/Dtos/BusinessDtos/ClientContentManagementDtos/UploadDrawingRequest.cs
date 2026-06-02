using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UploadDrawingRequest
{
    public Guid ProjectId { get; set; }
    public string Type { get; set; } = "TwoD";
    public string? Title { get; set; }
    public IFormFile File { get; set; } = null!;
}
