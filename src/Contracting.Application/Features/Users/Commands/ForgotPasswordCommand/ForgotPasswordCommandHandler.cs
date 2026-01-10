using Contracting.Domain.Entities;
using Contracting.Shared.Resources;
using Emails.SendGrid.Services;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IEmailService emailService,
        IStringLocalizer<SharedResources> localizer,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _emailService = emailService;
        _localizer = localizer;
        _configuration = configuration;
    }

    public async Task<ErrorOr<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        
        if (user == null)
        {
            // Return success even if user doesn't exist (security best practice - don't reveal if email exists)
            return "If the email exists in our system, a password reset link has been sent.";
        }

        // Generate password reset token
        var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        // Get base URL from configuration
        var baseUrl = _configuration["AppSettings:FrontendBaseUrl"] ?? "https://localhost:3000";
        var resetPath = _configuration["AppSettings:ResetPasswordPath"] ?? "/reset-password";
        
        // Create reset URL
        var resetUrl = $"{baseUrl}{resetPath}?token={Uri.EscapeDataString(resetToken)}&email={Uri.EscapeDataString(user.Email!)}";
        
        // Prepare email replacements
        var replacements = new Dictionary<string, string>
        {
            { "UserName", user.FullName ?? user.Email ?? "User" },
            { "ResetUrl", resetUrl },
            { "ExpiryHours", "24" }
        };

        // Send password reset email
        try
        {
            await _emailService.SendEmailAsync(
                user.Email!,
                "Reset Your Password - Contracting System",
                "reset-password.html",
                replacements,
                cancellationToken);
        }
        catch (Exception ex)
        {
            // Log the error but don't reveal to user
            return Error.Failure("Failed to send password reset email. Please try again later.");
        }

        return "If the email exists in our system, a password reset link has been sent.";
    }
}
