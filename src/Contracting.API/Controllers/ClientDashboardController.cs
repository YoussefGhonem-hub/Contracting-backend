using Contracting.API.Controllers.Shared;
using Contracting.Domain.Common.Enums;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Common;
using Contracting.Shared.Common.Enums;
using Contracting.Shared.CurrentUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contracting.API.Controllers;

[Route("api/client/dashboard")]
[ApiController]
[Authorize]
public class ClientDashboardController : APIBaseController
{
    private readonly ApplicationDbContext _db;

    public ClientDashboardController(ApplicationDbContext db)
    {
        _db = db;
    }

    // =========================================================================
    // GET /api/client/dashboard/pending-actions
    // Returns all pending actions for the authenticated client
    // =========================================================================
    [HttpGet("pending-actions")]
    public async Task<IActionResult> GetPendingActions(CancellationToken ct)
    {
        if (!Guid.TryParse(CurrentUser.UserId, out var userId))
            return Unauthorized();

        // Resolve client's project IDs
        var projectIds = await _db.ClientProjects
            .Where(cp => cp.Client.ApplicationUserId == userId && !cp.IsDeleted)
            .Select(cp => cp.ProjectId)
            .ToListAsync(ct);

        if (projectIds.Count == 0)
            return Ok(new { totalPendingActions = 0, actions = Array.Empty<object>() });

        // 1. Pending Variation Orders (awaiting client approval)
        var pendingVOs = await _db.VariationOrders
            .Where(v => projectIds.Contains(v.ProjectId) && v.Status == VOStatus.Pending && !v.IsDeleted)
            .Select(v => new { v.Cost })
            .ToListAsync(ct);

        var pendingVOCount  = pendingVOs.Count;
        var pendingVOAmount = pendingVOs.Sum(v => v.Cost);

        // 2. Unpaid / Partially Paid Invoices
        var pendingInvoices = await _db.ProjectInvoices
            .Where(i => projectIds.Contains(i.ProjectId)
                     && (i.Status == PaymentStatus.Pending || i.Status == PaymentStatus.PartiallyPaid)
                     && !i.IsDeleted)
            .Select(i => new { RemainingAmount = i.TotalValue - i.PaidAmount })
            .ToListAsync(ct);

        var pendingInvoiceCount  = pendingInvoices.Count;
        var pendingInvoiceAmount = pendingInvoices.Sum(i => i.RemainingAmount);

        // 3. Overdue Invoices (DueDate passed, still unpaid)
        var now = DateTimeHelper.Now;
        var overdueInvoiceCount = await _db.ProjectInvoices
            .CountAsync(i => projectIds.Contains(i.ProjectId)
                          && i.DueDate.HasValue && i.DueDate < now
                          && (i.Status == PaymentStatus.Pending || i.Status == PaymentStatus.PartiallyPaid)
                          && !i.IsDeleted, ct);

        // 4. Unread Chat Messages across client's chat groups
        var clientGroupIds = await _db.ChatGroupMembers
            .Where(m => m.ApplicationUserId == userId && !m.IsDeleted)
            .Select(m => m.ChatGroupId)
            .ToListAsync(ct);

        var unreadMessageCount = clientGroupIds.Count > 0
            ? await _db.ChatMessages
                .CountAsync(m => clientGroupIds.Contains(m.ChatGroupId)
                              && !m.IsRead
                              && m.SenderId != userId, ct)
            : 0;

        // Build actions list
        var actions = new List<PendingActionDto>();

        if (pendingVOCount > 0)
            actions.Add(new PendingActionDto
            {
                Type   = "VariationOrderApproval",
                Title  = "Variation Orders Awaiting Your Approval",
                Count  = pendingVOCount,
                Amount = pendingVOAmount
            });

        if (pendingInvoiceCount > 0)
            actions.Add(new PendingActionDto
            {
                Type   = "InvoiceDue",
                Title  = "Outstanding Invoices",
                Count  = pendingInvoiceCount,
                Amount = pendingInvoiceAmount
            });

        if (overdueInvoiceCount > 0)
            actions.Add(new PendingActionDto
            {
                Type  = "InvoiceOverdue",
                Title = "Overdue Invoices",
                Count = overdueInvoiceCount
            });

        if (unreadMessageCount > 0)
            actions.Add(new PendingActionDto
            {
                Type  = "UnreadMessage",
                Title = "Unread Messages",
                Count = unreadMessageCount
            });

        return Ok(new
        {
            totalPendingActions = actions.Sum(a => a.Count),
            actions
        });
    }

    private sealed class PendingActionDto
    {
        public string Type   { get; init; } = default!;
        public string Title  { get; init; } = default!;
        public int    Count  { get; init; }
        public decimal? Amount { get; init; }
    }
}
