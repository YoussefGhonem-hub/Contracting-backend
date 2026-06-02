using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.CreateLaborAttendance
{
    public class CreateLaborAttendanceCommandHandler : IRequestHandler<CreateLaborAttendanceCommand, ErrorOr<GetLaborAttendanceRequestDto>>
    {
        private readonly ILaborAttendanceService _service;
        public CreateLaborAttendanceCommandHandler(ILaborAttendanceService service) => _service = service;
        public Task<ErrorOr<GetLaborAttendanceRequestDto>> Handle(CreateLaborAttendanceCommand request, CancellationToken cancellationToken)
            => _service.CreateAsync(request.Dto);
    }
}
