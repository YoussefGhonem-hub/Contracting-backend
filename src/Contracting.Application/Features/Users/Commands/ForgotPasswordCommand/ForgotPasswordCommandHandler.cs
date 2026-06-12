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
using Microsoft.Extensions.Logging;

namespace Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IStringLocalizer<SharedResources> _localizer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IEmailService emailService,
        IStringLocalizer<SharedResources> localizer,
        IConfiguration configuration,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _userManager = userManager;
        _db = db;
        _emailService = emailService;
        _localizer = localizer;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ErrorOr<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        const string successMessage = "If the email exists in our system, a verification code has been sent.";

        var user = await _userManager.FindByEmailAsync(request.Request.Email);

        if (user == null)
        {
            // Security: never reveal whether an email is registered
            return successMessage;
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
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(15),
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

        // Send verification code email — log failures but always return 200
        // (never surface email delivery errors to the caller for security)
        try
        {
            var sent = await _emailService.SendEmailAsync(
                user.Email!,
                "Password Reset Verification Code - Sole System",
                "reset-password-code.html",
                replacements,
                cancellationToken);

            if (!sent)
                _logger.LogError("ForgotPassword: email delivery returned false for user {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ForgotPassword: exception sending reset code email for user {UserId}", user.Id);
        }

        return successMessage;
    }

    private string GenerateVerificationCode()
    {
        var random = new Random();
        return random.Next(100000, 999999).ToString();
    }
}
