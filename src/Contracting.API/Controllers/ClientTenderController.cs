using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Client.Tender.Query.GetClientTenderDocuments;
using Contracting.Shared.Constants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/client")]
[ApiController]
[Authorize]
public class ClientTenderController : APIBaseController
{
    private readonly IMediator _mediator;

    public ClientTenderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all tender package documents for a project assigned to the logged-in client.
    /// Used by the "Tender Package" mobile screen.
    /// </summary>
    [HttpGet("projects/{projectId:guid}/tender-documents")]
    public async Task<IActionResult> GetTenderDocuments(Guid projectId)
    {
        var query = new GetClientTenderDocumentsQuery(projectId);
        var result = await _mediator.Send(query);
        return result.Match(Ok, Problem);
    }
}
