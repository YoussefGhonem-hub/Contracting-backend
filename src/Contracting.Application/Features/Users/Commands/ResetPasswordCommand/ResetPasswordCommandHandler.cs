using Contracting.Domain.Entities;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.ResetPasswordCommand;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IStringLocalizer<SharedResources> localizer)
    {
        _userManager = userManager;
        _db = db;
        _localizer = localizer;
    }

    public async Task<ErrorOr<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        
        if (user == null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        // Verify passwords match
        if (request.Request.NewPassword != request.Request.ConfirmPassword)
        {
            return Error.Validation("Password.Mismatch", "Passwords do not match.");
        }

        // Find the verified reset code
        var resetCode = await _db.PasswordResetCodes
            .Where(c => c.UserId == user.Id 
                && c.Code == request.Request.Code 
                && c.IsVerified 
                && !c.IsUsed 
                && !c.IsDeleted)
            .OrderByDescending(c => c.VerifiedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetCode == null)
        {
            return Error.Validation("Code.NotVerified", "Invalid or unverified code. Please verify your code first.");
        }

        // Check if code is expired (15 minutes from creation)
        if (resetCode.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return Error.Validation("Code.Expired", "Verification code has expired. Please request a new one.");
        }

        // Check if code was verified recently (within 10 minutes)
        if (resetCode.VerifiedAt.HasValue && resetCode.VerifiedAt.Value.AddMinutes(10) < DateTimeOffset.UtcNow)
        {
            return Error.Validation("Code.VerificationExpired", "Code verification has expired. Please verify again.");
        }

        // Remove the password (reset it)
        var removePasswordResult = await _userManager.RemovePasswordAsync(user);
        if (!removePasswordResult.Succeeded)
        {
            return Error.Failure("Failed to reset password. Please try again.");
        }

        // Add the new password
        var addPasswordResult = await _userManager.AddPasswordAsync(user, request.Request.NewPassword);
        
        if (!addPasswordResult.Succeeded)
        {
            var errors = addPasswordResult.Errors.Select(e => Error.Validation("Password.Reset", e.Description)).ToList();
            return errors.First(); // Return first error
        }

        // Mark the code as used
        resetCode.IsUsed = true;
        resetCode.UsedAt = DateTimeOffset.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return "Password has been reset successfully. You can now login with your new password.";
    }
}
