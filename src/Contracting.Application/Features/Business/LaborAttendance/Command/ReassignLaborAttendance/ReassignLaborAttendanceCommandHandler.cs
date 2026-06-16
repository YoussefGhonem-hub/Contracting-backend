using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.ReassignLaborAttendance
{
    public class ReassignLaborAttendanceCommandHandler : IRequestHandler<ReassignLaborAttendanceCommand, ErrorOr<GetLaborAttendanceRequestDto>>
    {
        private readonly ILaborAttendanceService _service;
        public ReassignLaborAttendanceCommandHandler(ILaborAttendanceService service) => _service = service;

        public Task<ErrorOr<GetLaborAttendanceRequestDto>> Handle(ReassignLaborAttendanceCommand request, CancellationToken cancellationToken)
            => _service.ReassignAsync(request.Id, request.Dto);
    }
}
