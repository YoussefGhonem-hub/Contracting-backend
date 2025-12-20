using Contracting.Application.Resources;
using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.LoginUserCommand;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.UserNameOrEmail)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MaximumLength(256).WithMessage(localizer[SharedResourcesKeys.UserNameLength]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MaximumLength(128).WithMessage(localizer[SharedResourcesKeys.PasswordLength]);
        // RememberMe is a bool, no validation needed.
    }
}