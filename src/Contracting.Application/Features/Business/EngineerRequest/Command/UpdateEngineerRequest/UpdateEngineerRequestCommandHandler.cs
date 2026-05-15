using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.EngineerRequestDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest
{
    public class UpdateEngineerRequestCommandHandler : IRequestHandler<UpdateEngineerRequestCommand, ErrorOr<GetAllEngineerRequestDto>>
    {
        private readonly IEngineerRequestService _service;

        public UpdateEngineerRequestCommandHandler(IEngineerRequestService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetAllEngineerRequestDto>> Handle(UpdateEngineerRequestCommand request, CancellationToken cancellationToken)
        {
            return await _service.UpdateEngineerRequestAsync(request.Request);
        }
    }
}