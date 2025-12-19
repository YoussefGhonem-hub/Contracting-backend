using Contracting.Application.Common;
using Contracting.Application.Features.Users.Commands.LoginUserCommand;
using Contracting.Application.Features.Users.Commands.RefreshTokenCommand;
using Contracting.Application.Features.Users.Commands.RegisterUserCommand;
using Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<ActionResult<Result<AuthResponse>>> Register(RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request));
        if (!result.Succeeded) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<Result<TokenPairResponse>>> Login(LoginUserCommand request)
    {
        var result = await _mediator.Send(request);
        if (!result.Succeeded) return Unauthorized(result);
        return Ok(result);
    }

    // POST: api/auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }

    // POST: api/auth/revoke
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RevokeRefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Succeeded ? Ok(result) : BadRequest(result);
    }
}
