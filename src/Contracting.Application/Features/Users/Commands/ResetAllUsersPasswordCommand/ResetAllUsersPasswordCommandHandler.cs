using Contracting.Domain.Entities;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Contracting.Application.Features.Users.Commands.ResetAllUsersPasswordCommand;

public class ResetAllUsersPasswordCommandHandler : IRequestHandler<ResetAllUsersPasswordCommand, ErrorOr<string>>
{
    private readonly UserManager<ApplicationUser> _userManager;

    public ResetAllUsersPasswordCommandHandler(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    public async Task<ErrorOr<string>> Handle(ResetAllUsersPasswordCommand request, CancellationToken cancellationToken)
    {
        if (request.Request.NewPassword != request.Request.ConfirmPassword)
        {
            return Error.Validation("Password.Mismatch", "Passwords do not match.");
        }

        var users = await _userManager.Users.ToListAsync(cancellationToken);

        var failedUsers = new List<string>();

        foreach (var user in users)
        {
            var removeResult = await _userManager.RemovePasswordAsync(user);
            if (!removeResult.Succeeded)
            {
                failedUsers.Add(user.Email ?? user.Id.ToString());
                continue;
            }

            var addResult = await _userManager.AddPasswordAsync(user, request.Request.NewPassword);
            if (!addResult.Succeeded)
            {
                failedUsers.Add(user.Email ?? user.Id.ToString());
            }
        }

        if (failedUsers.Count > 0)
        {
            return Error.Failure("Password.PartialFailure",
                $"Password reset failed for {failedUsers.Count} user(s): {string.Join(", ", failedUsers)}");
        }

        return $"Password has been reset successfully for all {users.Count} users.";
    }
}
