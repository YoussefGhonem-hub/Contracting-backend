using Contracting.API.Controllers.Shared;
using Contracting.Application.Common;
using Contracting.Application.Features.Users.Commands.FCMTokenNotification;
using Contracting.Application.Features.Users.Commands.LoginUserCommand;
using Contracting.Application.Features.Users.Commands.RefreshTokenCommand;
using Contracting.Application.Features.Users.Commands.RegisterUserCommand;
using Contracting.Application.Features.Users.Commands.RemoveFCMTokenNotification;
using Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;
using Contracting.Domain.Entities.helper;
using ErrorOr;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Client;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Contracting.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : APIBaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request));

        return result.Match(
            authResult => Ok(authResult),
            errors => Problem(errors)
        );       
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginUserCommand(request));
        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
        );
    }

    // POST: api/auth/refresh
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
                    value => Ok(value),
                    errors => Problem(errors)
        );
    }

    // POST: api/auth/revoke
    [HttpPost("revoke")]
    [Authorize]
    public async Task<IActionResult> Revoke([FromBody] RevokeRefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
       );
    }

    [HttpPost("fcm-token")]
    [Authorize]
    public async Task<IActionResult> SaveFcmToken([FromBody] string fcmToken)
    {
        var command = new FCMTokenNotificationCommand(fcmToken); 
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
       );
    }
    [HttpDelete("fcm-token")]
    public async Task<IActionResult> DeleteFcmToken([FromBody] string fcmToken)
    {
        var command = new RemoveFCMTokenNotificationCommand(fcmToken);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
       );
    }
}
