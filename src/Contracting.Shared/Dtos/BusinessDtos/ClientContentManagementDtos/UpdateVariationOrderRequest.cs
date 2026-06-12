namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdateVariationOrderRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public decimal Cost { get; set; }
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
