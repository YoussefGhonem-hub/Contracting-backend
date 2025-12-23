using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.PriorityDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.UpdatePriority
{
    public class UpdatePriorityCommandHandler : IRequestHandler<UpdatePriorityCommand, ErrorOr<GetDropDownPriorityDto>>
    {
        private readonly IPriorityService _service;

        public UpdatePriorityCommandHandler(IPriorityService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDropDownPriorityDto>> Handle(UpdatePriorityCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.UpdatePriorityAsync(request.Priority);
            
            return result is null
                ? Error.NotFound("Priority not found.")
                : result;
        }
    }
}