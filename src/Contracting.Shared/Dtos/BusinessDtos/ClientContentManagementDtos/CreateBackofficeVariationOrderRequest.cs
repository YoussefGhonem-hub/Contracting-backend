using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class CreateBackofficeVariationOrderRequest
{
    public Guid ProjectId { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
    public ICollection<IFormFile>? Attachments { get; set; }
}
