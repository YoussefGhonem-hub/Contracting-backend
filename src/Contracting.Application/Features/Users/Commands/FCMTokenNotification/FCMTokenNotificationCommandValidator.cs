using FluentValidation;

namespace Contracting.Application.Features.Users.Commands.FCMTokenNotification
{
    public class FCMTokenNotificationCommandValidator : AbstractValidator<FCMTokenNotificationCommand>
    {
        public FCMTokenNotificationCommandValidator()
        {
            RuleFor(x => x.token).NotEmpty().WithMessage("Token is required.");
        }
    }
}