using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UploadTenderDocumentRequest
{
    public Guid ProjectId { get; set; }
    public string? Title { get; set; }
    public IFormFile File { get; set; } = null!;
}
