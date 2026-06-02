using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.TakeActionLaborAttendance
{
    public class TakeActionLaborAttendanceCommandHandler : IRequestHandler<TakeActionLaborAttendanceCommand, ErrorOr<GetLaborAttendanceRequestDto>>
    {
        private readonly ILaborAttendanceService _service;
        public TakeActionLaborAttendanceCommandHandler(ILaborAttendanceService service) => _service = service;
        public Task<ErrorOr<GetLaborAttendanceRequestDto>> Handle(TakeActionLaborAttendanceCommand request, CancellationToken cancellationToken)
            => _service.TakeActionAsync(request.Id, request.ActionDto);
    }
}
