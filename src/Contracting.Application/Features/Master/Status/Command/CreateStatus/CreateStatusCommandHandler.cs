using Contracting.Application.Features.Master.Status.Command.CreateStatus;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.StatusDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Status.Command.CreatePriority
{
    public class CreateStatusCommandHandler : IRequestHandler<CreateStatusCommand, ErrorOr<GetDropDownStatusDto>>
    {
        private readonly IStatueService _service;

        public CreateStatusCommandHandler(IStatueService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<GetDropDownStatusDto>> Handle(CreateStatusCommand request, CancellationToken cancellationToken)
        {
            var result = await _service.CreateStatusAsync(request.Status);
            
            return result is null
                ? Error.Failure("Could not create Status.")
                : result;
        }
    }
}