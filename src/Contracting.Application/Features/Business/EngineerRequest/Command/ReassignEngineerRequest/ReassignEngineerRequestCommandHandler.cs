using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.ReassignEngineerRequest
{
    public class ReassignEngineerRequestCommandHandler : IRequestHandler<ReassignEngineerRequestCommand, ErrorOr<GenericResponse>>
    {
        private readonly IEngineerRequestService _service;

        public ReassignEngineerRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(ReassignEngineerRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.ReassignEngineerRequestAsync(
                request.RequestId,
                Guid.Parse(CurrentUser.UserId),
                request.Dto);

            return result.Success
                ? result
                : Error.Failure(result.Message ?? "You are not authorized or the request cannot be reassigned.");
        }
    }
}
