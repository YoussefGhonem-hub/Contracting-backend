using FluentValidation;

namespace Contracting.Application.Features.Users.Commands.RevokeRefreshTokenCommand;

public sealed class RevokeRefreshTokenCommandValidator : AbstractValidator<RevokeRefreshTokenCommand>
{
    public RevokeRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MaximumLength(1024).WithMessage("Refresh token length is too long.");

        RuleFor(x => x.Reason)
            .MaximumLength(512).WithMessage("Reason cannot exceed 512 characters.")
            .When(x => x.Reason is not null);
    }
}