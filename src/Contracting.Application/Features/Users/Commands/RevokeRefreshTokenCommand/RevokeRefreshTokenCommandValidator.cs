using Contracting.Application.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;

public sealed class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
            .MaximumLength(1024).WithMessage(localizer[SharedResourcesKeys.RefreshTokenLength]);

        RuleFor(x => x.Reason)
            .MaximumLength(512).WithMessage(localizer[SharedResourcesKeys.ResonseLength])
            .When(x => x.Reason is not null);
    }
}