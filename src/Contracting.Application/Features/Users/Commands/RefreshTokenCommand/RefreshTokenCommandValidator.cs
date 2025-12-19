using FluentValidation;

namespace Contracting.Application.Features.Users.Commands.RefreshTokenCommand;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required.")
            .MaximumLength(1024).WithMessage("Refresh token length is too long.");
    }
}