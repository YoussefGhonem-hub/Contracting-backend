namespace Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;

public class GetClientInvoicesDto
{
    // Contract financial summary (shown on the invoices summary card).
    public decimal InitialContractValue { get; set; }
    public decimal ApprovedVariations { get; set; }
    public decimal TotalContractValue { get; set; }

    public decimal TotalValue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingAmount { get; set; }
    public int SettledPercent { get; set; }
    public List<GetClientInvoiceListItemDto> Invoices { get; set; } = new();
}
