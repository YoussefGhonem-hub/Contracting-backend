using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class CreateMonthlyReportRequest
{
    public Guid ProjectId { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public string? Title { get; set; }
    public string? WorkProgress { get; set; }
    public ICollection<IFormFile>? Attachments { get; set; }
}
