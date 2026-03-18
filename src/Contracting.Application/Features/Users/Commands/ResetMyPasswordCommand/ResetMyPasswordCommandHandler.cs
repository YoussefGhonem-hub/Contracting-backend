using Contracting.Domain.Entities;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Application.Features.Users.Commands.ResetMyPasswordCommand;

public class ResetMyPasswordCommandHandler : IRequestHandler<ResetMyPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetMyPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<string>> Handle(ResetMyPasswordCommand request, CancellationToken cancellationToken)
    {
        if (CurrentUser.Id == Guid.Empty)
        {
            return Error.Unauthorized("User.NotAuthenticated", "Not authenticated.");
        }

        if (request.Request.NewPassword != request.Request.ConfirmPassword)
        {
            return Error.Validation("Password.Mismatch", "Passwords do not match.");
        }

        var user = await _userManager.FindByIdAsync(CurrentUser.Id.ToString());
        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        var removeResult = await _userManager.RemovePasswordAsync(user);
        if (!removeResult.Succeeded)
        {
            return Error.Failure("Password.RemoveFailed", "Failed to reset password. Please try again.");
        }

        var addResult = await _userManager.AddPasswordAsync(user, request.Request.NewPassword);
        if (!addResult.Succeeded)
        {
            return addResult.Errors.Select(e => Error.Validation("Password.Reset", e.Description)).First();
        }

        return "Password has been reset successfully.";
    }
}
