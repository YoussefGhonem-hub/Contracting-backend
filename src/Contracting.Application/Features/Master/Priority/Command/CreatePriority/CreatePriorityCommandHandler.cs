using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.PriorityDto;
using Contracting.Shared.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Priority.Command.CreatePriority
{
    public class CreatePriorityCommandHandler : IRequestHandler<CreatePriorityCommand, ErrorOr<GetDropDownPriorityDto>>
    {
        private readonly IPriorityService _service;

        public CreatePriorityCommandHandler(IPriorityService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDropDownPriorityDto>> Handle(CreatePriorityCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreatePriorityAsync(request.Priority);
            
            return result is null
                ? Error.Failure("Could not create priority.")
                : result;
        }
    }
}