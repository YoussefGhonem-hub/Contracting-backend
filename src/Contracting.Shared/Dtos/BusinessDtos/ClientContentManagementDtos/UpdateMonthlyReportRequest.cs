using Microsoft.AspNetCore.Http;

namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdateMonthlyReportRequest
{
    public int? Month { get; set; }
    public int? Year { get; set; }
    public string? Title { get; set; }
    public string? WorkProgress { get; set; }

    /// <summary>New attachments to append to the report.</summary>
    public ICollection<IFormFile>? Attachments { get; set; }

    /// <summary>Ids of existing attachments to remove (their stored files are deleted too).</summary>
    public List<Guid>? RemoveAttachmentIds { get; set; }
}
