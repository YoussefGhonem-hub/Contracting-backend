using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.RefreshTokenCommand;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MaximumLength(1024).WithMessage(localizer[SharedResourcesKeys.RefreshTokenLength]);
    }
}