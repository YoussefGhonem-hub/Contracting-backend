using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.FCMTokenNotification
{
    public class FCMTokenNotificationCommandHandler : IRequestHandler<FCMTokenNotificationCommand, ErrorOr<bool>>
    {
        private readonly INotificationService _service;

        public FCMTokenNotificationCommandHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(FCMTokenNotificationCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.GenerateToken(request.token);
            
            return result is false
                ? Error.Failure("Could not generate token request.")
                : result;
        }
    }
}