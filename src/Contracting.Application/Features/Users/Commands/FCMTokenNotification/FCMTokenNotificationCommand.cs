using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.FCMTokenNotification
{
    public record FCMTokenNotificationCommand(string token) : IRequest<ErrorOr<bool>>;
}