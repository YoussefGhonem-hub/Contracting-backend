using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdateTenderDocumentRequest
{
    public string? Title { get; set; }

    /// <summary>Optional replacement file. When provided, the stored file is replaced; otherwise only metadata changes.</summary>
    public IFormFile? File { get; set; }
}
