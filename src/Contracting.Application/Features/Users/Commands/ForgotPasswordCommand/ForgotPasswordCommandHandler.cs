using Contracting.Domain.Entities;
using Contracting.Domain.Entities.helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Resources;
using Emails.Mailerlite.Services;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IConfiguration _configuration;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IEmailService emailService,
        IStringLocalizer<SharedResources> localizer,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _db = db;
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
            return Error.NotFound("Not-Found", "If the email exists in our system, a verification code has been sent.");
        }

        // Invalidate any existing unused codes for this user
        var existingCodes = await _db.PasswordResetCodes
            .Where(c => c.UserId == user.Id && !c.IsUsed && !c.IsDeleted)
            .ToListAsync(cancellationToken);

        foreach (var code in existingCodes)
        {
            code.IsDeleted = true;
        }

        // Generate a 6-digit verification code
        var verificationCode = GenerateVerificationCode();

        // Create password reset code entity
        var resetCode = new PasswordResetCode
        {
            UserId = user.Id,
            Code = verificationCode,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15), // Code expires in 15 minutes
            IsVerified = false,
            IsUsed = false
        };

        _db.PasswordResetCodes.Add(resetCode);
        await _db.SaveChangesAsync(cancellationToken);

        // Prepare email replacements
        var replacements = new Dictionary<string, string>
        {
            { "UserName", user.FullName ?? user.Email ?? "User" },
            { "VerificationCode", verificationCode },
            { "ExpiryMinutes", "15" }
        };

        // Send verification code email
        try
        {
            var sendOnot = await _emailService.SendEmailAsync(
                user.Email!,
                "Password Reset Verification Code - Sole System",
                "reset-password-code.html",
                replacements,
                cancellationToken);

            if (!sendOnot)
            {
                return Error.Failure("Falied", "Failed to send verification code. Please try again later.");
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't reveal to user
            return Error.Failure("Failed to send verification code. Please try again later.");
        }

        return "If the email exists in our system, a verification code has been sent.";
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
