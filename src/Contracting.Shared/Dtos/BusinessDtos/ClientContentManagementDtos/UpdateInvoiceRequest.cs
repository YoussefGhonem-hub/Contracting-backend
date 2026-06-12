namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdateInvoiceRequest
{
    public string? Title { get; set; }
    public decimal TotalValue { get; set; }
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset? IssueDate { get; set; }
    public DateTimeOffset? DueDate { get; set; }
}
