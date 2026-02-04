using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Users.Commands.UploadUserSignature;
using Contracting.Application.Features.Users.Queries.GetSignatureByReportId;
using Contracting.Application.Features.Users.Queries.GetUserSignature;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[Route("api/users")]
[ApiController]
[Authorize]
public class UserSignatureController : APIBaseController
{
    private readonly IMediator _mediator;

    public UserSignatureController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("signature")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSignature()
    {
        var result = await _mediator.Send(new GetUserSignatureQuery());
        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpPost("signature")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UploadSignature(IFormFile? signature)
    {
        var command = new UploadUserSignatureCommand(signature);
        var result = await _mediator.Send(command);
        return result.Match<IActionResult>(Ok, Problem);
    }

    [HttpGet("GetAnonymousSignature/{reportId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [AllowAnonymous]
    public async Task<IActionResult> GetAnonymousSignature(Guid reportId)
    {
        var result = await _mediator.Send(new GetSignatureByReportIdQuery(reportId));
        return result.Match<IActionResult>(Ok, Problem);
    }
}
