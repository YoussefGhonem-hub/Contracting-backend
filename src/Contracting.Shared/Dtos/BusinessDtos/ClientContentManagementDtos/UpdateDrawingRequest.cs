using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdateDrawingRequest
{
    public string? Title { get; set; }

    /// <summary>Replace the file. If null the existing file is kept.</summary>
    public IFormFile? File { get; set; }
}
