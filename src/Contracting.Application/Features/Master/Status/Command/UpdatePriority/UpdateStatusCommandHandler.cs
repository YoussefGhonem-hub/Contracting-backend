using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.UpdateStatus
{
    public class UpdateStatusCommandHandler : IRequestHandler<UpdateStatusCommand, ErrorOr<GetDropDownStatusDto>>
    {
        private readonly IStatueService _service;

        public UpdateStatusCommandHandler(IStatueService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDropDownStatusDto>> Handle(UpdateStatusCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdateStatusAsync(request.Status);
            
            return result is null
                ? Error.NotFound("Status not found.")
                : result;
        }
    }
}