using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateInternalRequest
{
    public class CreateInternalRequestCommandHandler : IRequestHandler<CreateInternalRequestCommand, ErrorOr<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;

        public CreateInternalRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetAllEngineerRequestDto>> Handle(CreateInternalRequestCommand request, CancellationToken cancellationToken)
        {
            return await _service.CreateInternalRequestAsync(request.Request);
        }
    }
}
