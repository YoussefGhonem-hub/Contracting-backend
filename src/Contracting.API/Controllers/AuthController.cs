using Contracting.API.Controllers.Shared;
using Contracting.Application.Features.Users.Commands.AdminResetOfficeUserPasswordCommand;
using Contracting.Application.Features.Users.Commands.AdminResetPasswordCommand;
using Contracting.Application.Features.Users.Commands.FCMTokenNotification;
using Contracting.Application.Features.Users.Commands.ResetAllUsersPasswordCommand;
using Contracting.Application.Features.Users.Commands.ResetMyPasswordCommand;
using Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;
using Contracting.Application.Features.Users.Commands.LoginUserCommand;
using Contracting.Application.Features.Users.Commands.RefreshTokenCommand;
using Contracting.Application.Features.Users.Commands.RegisterUserCommand;
using Contracting.Application.Features.Users.Commands.RemoveFCMTokenNotification;
using Contracting.Application.Features.Users.Commands.ResetPasswordCommand;
using Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;
using Contracting.Application.Features.Users.Commands.VerifyResetCodeCommand;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contracting.API.Controllers;

/// <summary>
/// Controller for authentication and user account management.
/// Handles user registration, login, password reset, and token management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : APIBaseController
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <remarks>
    /// Creates a new user account with the provided credentials and profile information.
    /// Returns authentication tokens upon successful registration.
    /// </remarks>
    /// <param name="request">The registration details including email, password, and profile info</param>
    /// <returns>Authentication result with access token and refresh token</returns>
    /// <response code="200">Registration successful - returns authentication tokens</response>
    /// <response code="400">Invalid request - validation errors or email already exists</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterUserCommand(request));

        return result.Match(
            authResult => Ok(authResult),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Authenticates a user and returns access tokens.
    /// </summary>
    /// <remarks>
    /// Validates user credentials and returns JWT access token and refresh token.
    /// The access token should be included in the Authorization header for subsequent requests.
    /// 
    /// Example usage:
    /// ```
    /// Authorization: Bearer {access_token}
    /// ```
    /// </remarks>
    /// <param name="request">Login credentials (email and password)</param>
    /// <returns>Authentication result with access token, refresh token, and user info</returns>
    /// <response code="200">Login successful - returns authentication tokens</response>
    /// <response code="400">Invalid credentials or account issues</response>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginUserCommand(request));
        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Refreshes the access token using a valid refresh token.
    /// </summary>
    /// <remarks>
    /// Use this endpoint when the access token expires. Provide the refresh token
    /// to obtain a new access token without requiring the user to login again.
    /// 
    /// **Note:** Refresh tokens have a longer expiration than access tokens.
    /// </remarks>
    /// <param name="command">The refresh token request containing the current refresh token</param>
    /// <returns>New access token and refresh token</returns>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="400">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Match(
                    value => Ok(value),
                    errors => Problem(errors)
        );
    }

    /// <summary>
    /// Revokes a refresh token to logout the user.
    /// </summary>
    /// <remarks>
    /// Invalidates the specified refresh token, effectively logging out the user
    /// from that session. Use this for secure logout functionality.
    /// 
    /// **Requires authentication.**
    /// </remarks>
    /// <param name="command">The refresh token to revoke</param>
    /// <returns>Confirmation of token revocation</returns>
    /// <response code="200">Token revoked successfully</response>
    /// <response code="400">Invalid token</response>
    /// <response code="401">Unauthorized - User not authenticated</response>
    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Revoke([FromBody] RevokeRefreshTokenCommand command)
    {
        var result = await _mediator.Send(command);

        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
       );
    }

    /// <summary>
    /// Saves Firebase Cloud Messaging (FCM) token for push notifications.
    /// </summary>
    /// <remarks>
    /// Registers the device's FCM token to enable push notifications.
    /// Call this after login or when the FCM token is refreshed.
    /// 
    /// **Requires authentication.**
    /// </remarks>
    /// <param name="fcmToken">The FCM token from the client device</param>
    /// <returns>Confirmation of token registration</returns>
    /// <response code="200">FCM token saved successfully</response>
    /// <response code="400">Invalid token</response>
    /// <response code="401">Unauthorized - User not authenticated</response>
    [HttpPost("fcm-token")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SaveFcmToken([FromBody] string fcmToken)
    {
        var command = new FCMTokenNotificationCommand(fcmToken);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
       );
    }

    /// <summary>
    /// Removes Firebase Cloud Messaging (FCM) token to disable push notifications.
    /// </summary>
    /// <remarks>
    /// Unregisters the device's FCM token. Call this when the user logs out
    /// or disables notifications to stop receiving push notifications on the device.
    /// </remarks>
    /// <param name="fcmToken">The FCM token to remove</param>
    /// <returns>Confirmation of token removal</returns>
    /// <response code="200">FCM token removed successfully</response>
    /// <response code="400">Invalid token</response>
    [HttpDelete("fcm-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteFcmToken([FromBody] string fcmToken)
    {
        var command = new RemoveFCMTokenNotificationCommand(fcmToken);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(value),
            errors => Problem(errors)
       );
    }

    /// <summary>
    /// Initiates the forgot password flow by sending a reset code to user's email.
    /// </summary>
    /// <remarks>
    /// **Step 1 of password reset flow.**
    /// 
    /// Sends a 6-digit verification code to the user's registered email address.
    /// The code expires after a limited time (typically 15-30 minutes).
    /// 
    /// **Flow:**
    /// 1. Call `forgot-password` with user's email → Receives verification code via email
    /// 2. Call `verify-reset-code` with email and code → Validates the code
    /// 3. Call `reset-password` with email, code, and new password → Completes reset
    /// </remarks>
    /// <param name="request">The email address of the account to reset</param>
    /// <returns>Success message confirming email was sent</returns>
    /// <response code="200">Reset code sent successfully to email</response>
    /// <response code="400">Invalid email or user not found</response>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var command = new ForgotPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Verifies the password reset code sent to user's email.
    /// </summary>
    /// <remarks>
    /// **Step 2 of password reset flow.**
    /// 
    /// Validates the 6-digit verification code that was sent to the user's email.
    /// Call this before allowing the user to set a new password.
    /// 
    /// **Flow:**
    /// 1. Call `forgot-password` with user's email → Receives verification code via email
    /// 2. Call `verify-reset-code` with email and code → **Validates the code** ✓
    /// 3. Call `reset-password` with email, code, and new password → Completes reset
    /// </remarks>
    /// <param name="request">Email and the 6-digit verification code</param>
    /// <returns>Success message if code is valid</returns>
    /// <response code="200">Code verified successfully - proceed to reset password</response>
    /// <response code="400">Invalid or expired code</response>
    [HttpPost("verify-reset-code")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyResetCode([FromBody] VerifyResetCodeRequest request)
    {
        var command = new VerifyResetCodeCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Resets the user's password using the verified reset code.
    /// </summary>
    /// <remarks>
    /// **Step 3 of password reset flow (Final step).**
    /// 
    /// Sets a new password for the user after the reset code has been verified.
    /// The reset code must have been previously validated via `verify-reset-code`.
    /// 
    /// **Flow:**
    /// 1. Call `forgot-password` with user's email → Receives verification code via email
    /// 2. Call `verify-reset-code` with email and code → Validates the code
    /// 3. Call `reset-password` with email, code, and new password → **Completes reset** ✓
    /// 
    /// **Password requirements:**
    /// - Minimum 8 characters
    /// - At least one uppercase letter
    /// - At least one lowercase letter
    /// - At least one number
    /// - At least one special character
    /// </remarks>
    /// <param name="request">Email, verified reset code, and new password</param>
    /// <returns>Success message confirming password was reset</returns>
    /// <response code="200">Password reset successfully - user can now login with new password</response>
    /// <response code="400">Invalid code, expired code, or password doesn't meet requirements</response>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var command = new ResetPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Resets the currently authenticated user's password (no user ID needed).
    /// </summary>
    /// <param name="request">New password and confirm password</param>
    /// <returns>Success message confirming password was reset</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Validation errors or password doesn't meet requirements</response>
    /// <response code="401">User not authenticated</response>
    [HttpPost("reset-my-password")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ResetMyPassword([FromBody] ResetMyPasswordRequest request)
    {
        var command = new ResetMyPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Resets a user's password by an admin (by passing a new password directly).
    /// </summary>
    /// <param name="request">The user ID, new password, and confirm password</param>
    /// <returns>Success message confirming password was reset</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">Validation errors or password doesn't meet requirements</response>
    /// <response code="404">User not found</response>
    [HttpPost("admin-reset-password")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminResetPassword([FromBody] AdminResetPasswordRequest request)
    {
        var command = new AdminResetPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Resets an office engineer's password by an admin.
    /// </summary>
    /// <remarks>
    /// Only users with the "Office-engineer" role can have their password reset via this endpoint.
    /// </remarks>
    /// <param name="request">The user ID, new password, and confirm password</param>
    /// <returns>Success message confirming password was reset</returns>
    /// <response code="200">Password reset successfully</response>
    /// <response code="400">User is not an office engineer, or validation errors</response>
    /// <response code="404">User not found</response>
    [HttpPost("admin-reset-office-user-password")]
    [Authorize(Roles = "SuperAdmin,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdminResetOfficeUserPassword([FromBody] AdminResetOfficeUserPasswordRequest request)
    {
        var command = new AdminResetOfficeUserPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }

    /// <summary>
    /// Resets the password for ALL users in the system to the same new password.
    /// </summary>
    /// <remarks>
    /// **WARNING: This will change the password for every user.**
    /// Only SuperAdmin can use this endpoint.
    /// </remarks>
    /// <param name="request">New password and confirm password</param>
    /// <returns>Success message with the count of updated users</returns>
    /// <response code="200">All passwords reset successfully</response>
    /// <response code="400">Validation errors or partial failure</response>
    [HttpPost("reset-all-users-password")]
    [Authorize(Roles = "SuperAdmin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetAllUsersPassword([FromBody] ResetAllUsersPasswordRequest request)
    {
        var command = new ResetAllUsersPasswordCommand(request);
        var result = await _mediator.Send(command);
        return result.Match(
            value => Ok(new { Message = value }),
            errors => Problem(errors)
        );
    }
}
