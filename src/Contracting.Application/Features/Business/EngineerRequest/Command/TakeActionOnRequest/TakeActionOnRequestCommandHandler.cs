using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public class TakeActionRequestCommandHandler : IRequestHandler<TakeActionRequestCommand, ErrorOr<GenericResponse>>
    {
        private readonly IEngineerRequestService _service;

        public TakeActionRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(TakeActionRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.TakeActionOnRequestAsync(
                request.RequestId,
                Guid.Parse(CurrentUser.UserId),
                request.ActionDto
            );

            return result.Success
                ? result
                : Error.Failure(result.Message ?? "You are not authorized or the request cannot be actioned.");
        }
    }
}