using Contracting.Domain.Entities;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.VerifyResetCodeCommand;

public class VerifyResetCodeCommandHandler : IRequestHandler<VerifyResetCodeCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _db;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public VerifyResetCodeCommandHandler(
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext db,
        IStringLocalizer<SharedResources> localizer)
    {
        _userManager = userManager;
        _db = db;
        _localizer = localizer;
    }

    public async Task<ErrorOr<string>> Handle(VerifyResetCodeCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Request.Email);
        
        if (user == null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        // Find the reset code
        var resetCode = await _db.PasswordResetCodes
            .Where(c => c.UserId == user.Id 
                && c.Code == request.Request.Code 
                && !c.IsUsed 
                && !c.IsDeleted)
            .OrderByDescending(c => c.CreatedDate)
            .FirstOrDefaultAsync(cancellationToken);

        if (resetCode == null)
        {
            return Error.Validation("Code.Invalid", "Invalid or expired verification code.");
        }

        // Check if code is expired (15 minutes)
        if (resetCode.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return Error.Validation("Code.Expired", "Verification code has expired. Please request a new one.");
        }

        // Mark the code as verified
        resetCode.IsVerified = true;
        resetCode.VerifiedAt = DateTimeOffset.UtcNow;
        
        await _db.SaveChangesAsync(cancellationToken);

        return "Verification code is valid. You can now reset your password.";
    }
}
