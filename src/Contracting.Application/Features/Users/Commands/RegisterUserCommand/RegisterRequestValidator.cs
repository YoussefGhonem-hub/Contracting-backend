using Contracting.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.RegisterUserCommand;

public sealed class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MaximumLength(200).WithMessage(localizer[SharedResourcesKeys.FullNameLength]);

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .EmailAddress().WithMessage(localizer[SharedResourcesKeys.EmailInvalid])
            .MaximumLength(256).WithMessage(localizer[SharedResourcesKeys.EmailLength]);

        RuleFor(x => x.PhoneNumber)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MaximumLength(32).WithMessage(localizer[SharedResourcesKeys.PhoneNumberLength]);

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MinimumLength(6).WithMessage(localizer[SharedResourcesKeys.PasswordRange])
            .MaximumLength(128).WithMessage(localizer[SharedResourcesKeys.PasswordRange]);
    }
}