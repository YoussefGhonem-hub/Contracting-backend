using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Users.Commands.FCMTokenNotification;
using Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;
using Contracting.Application.Features.Users.Commands.LoginUserCommand;
using Contracting.Application.Features.Users.Commands.RefreshTokenCommand;
using Contracting.Application.Features.Users.Commands.RegisterUserCommand;
using Contracting.Application.Features.Users.Commands.RemoveFCMTokenNotification;
using Contracting.Application.Features.Users.Commands.ResetPasswordCommand;
using Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }
}
