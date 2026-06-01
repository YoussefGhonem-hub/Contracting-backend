namespace Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;

public class GetClientFinancialSummaryDto
{
    public decimal InitialContractValue { get; set; }
    public decimal ApprovedVariations { get; set; }
    public decimal TotalContractValue { get; set; }
    public decimal TotalPaid { get; set; }
    public decimal RemainingAmount { get; set; }
}
