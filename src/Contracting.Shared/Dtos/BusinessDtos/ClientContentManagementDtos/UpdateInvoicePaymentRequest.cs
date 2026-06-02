namespace Contracting.Shared.Dtos.BusinessDtos.ClientContentManagementDtos;

public class UpdateInvoicePaymentRequest
{
    public decimal PaidAmount { get; set; }
    public string? Notes { get; set; }
}
