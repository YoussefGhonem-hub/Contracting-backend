using Contracting.Domain.Entities;
using Contracting.Shared.Resources;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.ResetPasswordCommand;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IStringLocalizer<SharedResources> _localizer;

    public ResetPasswordCommandHandler(
        UserManager<ApplicationUser> userManager,
        IStringLocalizer<SharedResources> localizer)
    {
        _userManager = userManager;
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

        // Reset the password
        var result = await _userManager.ResetPasswordAsync(user, request.Request.Token, request.Request.NewPassword);
        
        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => Error.Validation("Password.Reset", e.Description)).ToList();
            return errors.First(); // Return first error
        }

        return "Password has been reset successfully. You can now login with your new password.";
    }
}
