using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public class TakeActionRequestCommandHandler : IRequestHandler<TakeActionRequestCommand, ErrorOr<bool>>
    {
        private readonly IEngineerRequestService _service;

        public TakeActionRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<bool>> Handle(TakeActionRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.TakeActionOnRequestAsync(
                request.RequestId,
                Guid.Parse(CurrentUser.UserId),
                request.ActionDto
            );

            return result
                ? true
                : Error.Failure("You are not authorized or the request cannot be actioned.");
        }
    }
}