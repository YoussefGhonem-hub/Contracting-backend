using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.ClientDtos.InvoiceDtos;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Infrustructure.Features.client;

public class ClientInvoiceService : IClientInvoiceService
{
    private readonly ApplicationDbContext _db;

    public ClientInvoiceService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<GetClientInvoicesDto?> GetClientInvoicesAsync(
        Guid projectId,
        string? status,
        CancellationToken cancellationToken = default)
    {
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
            return null;

        var contractValue = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => p.ContractValue)
            .FirstOrDefaultAsync(cancellationToken);

        var query = _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(i => i.Status == parsedStatus);

        // Materialize the raw rows, then format the status in memory so we can return
        // user-friendly labels (and derive "Overdue") instead of raw enum names.
        var invoiceRows = await query
            .OrderBy(i => i.InvoiceNumber)
            .Select(i => new
            {
                i.Id,
                i.InvoiceNumber,
                i.Title,
                i.TotalValue,
                i.PaidAmount,
                i.Status,
                i.IssueDate,
                i.DueDate
            })
            .ToListAsync(cancellationToken);

        var invoices = invoiceRows.Select(i => new GetClientInvoiceListItemDto
        {
            Id = i.Id,
            InvoiceNumber = i.InvoiceNumber,
            Title = i.Title,
            TotalValue = i.TotalValue,
            PaidAmount = i.PaidAmount,
            Status = FormatInvoiceStatus(i.Status, i.DueDate),
            IssueDate = i.IssueDate,
            DueDate = i.DueDate
        }).ToList();

        // Contract financial summary: initial contract value + approved variations.
        var approvedVariations = await _db.VariationOrders
            .Where(vo => vo.ProjectId == projectId && vo.Status == VOStatus.Approved)
            .SumAsync(vo => vo.Cost, cancellationToken);

        var initialContractValue = contractValue ?? 0;
        var totalContractValue = initialContractValue + approvedVariations;

        // Aggregate paid amounts across ALL invoices for the project (ignore status filter for summary)
        var totalPaid = await _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId)
            .SumAsync(i => i.PaidAmount, cancellationToken);

        var remaining = totalContractValue - totalPaid;
        var settledPercent = totalContractValue > 0
            ? (int)Math.Round(totalPaid / totalContractValue * 100)
            : 0;

        return new GetClientInvoicesDto
        {
            InitialContractValue = initialContractValue,
            ApprovedVariations = approvedVariations,
            TotalContractValue = totalContractValue,
            TotalValue = totalContractValue,
            TotalPaid = totalPaid,
            RemainingAmount = remaining,
            SettledPercent = settledPercent,
            Invoices = invoices
        };
    }

    /// <summary>
    /// Maps a payment status to a user-friendly label. An unpaid/partially-paid invoice
    /// whose due date has passed is reported as "Overdue".
    /// </summary>
    private static string FormatInvoiceStatus(PaymentStatus status, DateTimeOffset? dueDate)
    {
        if (status != PaymentStatus.Paid
            && dueDate.HasValue
            && dueDate.Value < DateTimeHelper.Now)
            return "Overdue";

        return status switch
        {
            PaymentStatus.Paid => "Paid",
            PaymentStatus.PartiallyPaid => "Partially Paid",
            PaymentStatus.Pending => "Pending",
            _ => status.ToString()
        };
    }

    public async Task<GetClientFinancialSummaryDto?> GetClientFinancialSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        if (!await ClientProjectAccess.CanAccessProjectAsync(_db, projectId, cancellationToken))
            return null;

        var contractValue = await _db.Projects
            .Where(p => p.Id == projectId)
            .Select(p => p.ContractValue)
            .FirstOrDefaultAsync(cancellationToken);

        var approvedVariations = await _db.VariationOrders
            .Where(vo => vo.ProjectId == projectId && vo.Status == VOStatus.Approved)
            .SumAsync(vo => vo.Cost, cancellationToken);

        var totalPaid = await _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId)
            .SumAsync(i => i.PaidAmount, cancellationToken);

        var initialValue = contractValue ?? 0;
        var totalContract = initialValue + approvedVariations;

        return new GetClientFinancialSummaryDto
        {
            InitialContractValue = initialValue,
            ApprovedVariations = approvedVariations,
            TotalContractValue = totalContract,
            TotalPaid = totalPaid,
            RemainingAmount = totalContract - totalPaid
        };
    }
}
