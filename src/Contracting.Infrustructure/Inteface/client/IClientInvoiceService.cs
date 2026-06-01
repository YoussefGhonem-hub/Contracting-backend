using Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;

namespace Contracting.Infrustructure.Inteface.client;

public interface IClientInvoiceService
{
    /// <summary>
    /// Returns invoices for the project with aggregate summary (total paid, remaining, %).
    /// Pass null status to return All invoices.
    /// Returns null if the client does not own this project.
    /// </summary>
    Task<GetClientInvoicesDto?> GetClientInvoicesAsync(Guid projectId, string? status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the Contract Financial Summary for the project:
    /// initial contract value, approved variation orders, total paid, remaining.
    /// Returns null if the client does not own this project.
    /// </summary>
    Task<GetClientFinancialSummaryDto?> GetClientFinancialSummaryAsync(Guid projectId, CancellationToken cancellationToken = default);
}
