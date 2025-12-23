using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest
{
    public class CreateEngineerRequestCommandHandler : IRequestHandler<CreateEngineerRequestCommand, ErrorOr<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;

        public CreateEngineerRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetAllEngineerRequestDto>> Handle(CreateEngineerRequestCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateEngineerRequestAsync(request.Request);
            
            return result is null
                ? Error.Failure("Could not create engineer request.")
                : result;
        }
    }
}