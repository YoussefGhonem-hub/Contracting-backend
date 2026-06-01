using Contracting.Domain.Entities;
using Contracting.Shared.Constants;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Application.Features.Users.Commands.AdminResetOfficeUserPasswordCommand;

public class AdminResetOfficeUserPasswordCommandHandler : IRequestHandler<AdminResetOfficeUserPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminResetOfficeUserPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<string>> Handle(AdminResetOfficeUserPasswordCommand request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByIdAsync(request.Request.UserId.ToString());

        if (user is null)
        {
            return Error.NotFound("User.NotFound", "User not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(RoleNames.Officeengineer))
        {
            return Error.Validation("User.NotOfficeEngineer", "User is not an office engineer.");
        }

        if (request.Request.NewPassword != request.Request.ConfirmPassword)
        {
            return Error.Validation("Password.Mismatch", "Passwords do not match.");
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

        return "Office engineer password has been reset successfully.";
    }
}
