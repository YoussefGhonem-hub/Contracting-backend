using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.Invoice.Query.GetClientFinancialSummary;
using Contracting.Application.Features.Client.Invoice.Query.GetClientInvoices;
using Contracting.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/client")]
[ApiController]
[Authorize(Roles = RoleNames.Client)]
public class ClientInvoiceController : APIBaseController
{
    private readonly IMediator _mediator;

    public ClientInvoiceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get invoices for a project with payment summary (total paid, remaining, settled %).
    /// Used by the "Invoices &amp; Payments" mobile screen.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    /// <param name="status">Optional filter: Pending | Paid | PartiallyPaid. Omit or leave empty for All.</param>
    [HttpGet("projects/{projectId:guid}/invoices")]
    public async Task<IActionResult> GetInvoices(Guid projectId, [FromQuery] string? status = null)
    {
        var query = new GetClientInvoicesQuery(projectId, status);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Get the Contract Financial Summary for a project:
    /// initial contract value, approved variation orders, total paid, remaining amount.
    /// Used by the "Contract Financial Summary" mobile screen.
    /// </summary>
    /// <param name="projectId">The project ID.</param>
    [HttpGet("projects/{projectId:guid}/invoices/financial-summary")]
    public async Task<IActionResult> GetFinancialSummary(Guid projectId)
    {
        var query = new GetClientFinancialSummaryQuery(projectId);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }
}
