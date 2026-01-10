using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.ForgotPasswordCommand;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Valid email address is required");
    }
}
