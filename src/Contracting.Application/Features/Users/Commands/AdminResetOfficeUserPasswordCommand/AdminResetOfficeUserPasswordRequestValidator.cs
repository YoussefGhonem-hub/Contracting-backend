using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.AdminResetOfficeUserPasswordCommand;

public class AdminResetOfficeUserPasswordRequestValidator : AbstractValidator<AdminResetOfficeUserPasswordRequest>
{
    public AdminResetOfficeUserPasswordRequestValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required.");

        RuleFor(x => x.NewPassword)
            .NotEmpty()
            .MinimumLength(6)
            .WithMessage("Password must be at least 6 characters long.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
            .Equal(x => x.NewPassword)
            .WithMessage("Passwords must match.");
    }
}
