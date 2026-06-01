namespace Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;

public class GetClientInvoicesDto
{
    public decimal TotalValue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public int SettledPercent { get; set; }
    public List<GetClientInvoiceListItemDto> Invoices { get; set; } = new();
}
