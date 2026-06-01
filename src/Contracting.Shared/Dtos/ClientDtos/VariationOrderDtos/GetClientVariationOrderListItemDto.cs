namespace Contracting.Shared.Dtos.ClientDtos.VariationOrderDtos;

public class GetClientVariationOrderListItemDto
{
    public Guid Id { get; set; }
    public int? VONumber { get; set; }
    public string? Title { get; set; }
    public decimal Cost { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
