using Contracting.Infrustructure.Inteface.business;
using Contracting.Infrustructure.Inteface.Helper;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Users.Commands.RemoveFCMTokenNotification
{
    public class RemoveFCMTokenNotificationCommandHandler : IRequestHandler<RemoveFCMTokenNotificationCommand, ErrorOr<bool>>
    {
        private readonly INotificationService _service;

        public RemoveFCMTokenNotificationCommandHandler(INotificationService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(RemoveFCMTokenNotificationCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.RemoveToken(request.token);
            
            return result is false
                ? Error.Failure("Could not generate token request.")
                : result;
        }
    }
}