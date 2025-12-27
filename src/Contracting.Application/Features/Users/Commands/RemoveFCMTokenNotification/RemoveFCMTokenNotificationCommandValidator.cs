using FluentValidation;

namespace Contracting.Application.Features.Users.Commands.RemoveFCMTokenNotification
{
    public class RemoveFCMTokenNotificationCommandValidator : AbstractValidator<RemoveFCMTokenNotificationCommand>
    {
        public RemoveFCMTokenNotificationCommandValidator()
        {
            RuleFor(x => x.token).NotEmpty().WithMessage("Token is required.");
        }
    }
}