using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.RemoveFCMTokenNotification
{
    public record RemoveFCMTokenNotificationCommand(string token) : IRequest<ErrorOr<bool>>;
}