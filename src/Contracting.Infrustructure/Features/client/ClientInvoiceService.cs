using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Infrustructure.Persistence;
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
        var userId = CurrentUser.Id!.Value;

        var isClientProject = await _db.ClientProjects
            .AnyAsync(cp => cp.ProjectId == projectId
                         && cp.Client != null
                         && cp.Client.ApplicationUserId == userId,
                      cancellationToken);

        if (!isClientProject)
            return null;

        var query = _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<PaymentStatus>(status, ignoreCase: true, out var parsedStatus))
            query = query.Where(i => i.Status == parsedStatus);

        var invoices = await query
            .OrderBy(i => i.InvoiceNumber)
            .Select(i => new GetClientInvoiceListItemDto
            {
                Id = i.Id,
                InvoiceNumber = i.InvoiceNumber,
                Title = i.Title,
                TotalValue = i.TotalValue,
                PaidAmount = i.PaidAmount,
                Status = i.Status.ToString(),
                IssueDate = i.IssueDate,
                DueDate = i.DueDate
            })
            .ToListAsync(cancellationToken);

        // Aggregate totals across ALL invoices for the project (ignore status filter for summary)
        var allInvoices = await _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId)
            .Select(i => new { i.TotalValue, i.PaidAmount })
            .ToListAsync(cancellationToken);

        var totalValue = allInvoices.Sum(i => i.TotalValue);
        var totalPaid = allInvoices.Sum(i => i.PaidAmount);
        var remaining = totalValue - totalPaid;
        var settledPercent = totalValue > 0
            ? (int)Math.Round(totalPaid / totalValue * 100)
            : 0;

        return new GetClientInvoicesDto
        {
            TotalValue = totalValue,
            TotalPaid = totalPaid,
            RemainingAmount = remaining,
            SettledPercent = settledPercent,
            Invoices = invoices
        };
    }

    public async Task<GetClientFinancialSummaryDto?> GetClientFinancialSummaryAsync(
        Guid projectId,
        CancellationToken cancellationToken = default)
    {
        var userId = CurrentUser.Id!.Value;

        var project = await _db.ClientProjects
            .Where(cp => cp.ProjectId == projectId
                      && cp.Client != null
                      && cp.Client.ApplicationUserId == userId)
            .Select(cp => new { cp.Project.ContractValue })
            .FirstOrDefaultAsync(cancellationToken);

        if (project is null)
            return null;

        var approvedVariations = await _db.VariationOrders
            .Where(vo => vo.ProjectId == projectId && vo.Status == VOStatus.Approved)
            .SumAsync(vo => vo.Cost, cancellationToken);

        var totalPaid = await _db.ProjectInvoices
            .Where(i => i.ProjectId == projectId)
            .SumAsync(i => i.PaidAmount, cancellationToken);

        var initialValue = project.ContractValue ?? 0;
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
