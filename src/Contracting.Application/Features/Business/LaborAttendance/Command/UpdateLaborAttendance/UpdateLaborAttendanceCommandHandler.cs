using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.UpdateLaborAttendance
{
    public class UpdateLaborAttendanceCommandHandler : IRequestHandler<UpdateLaborAttendanceCommand, ErrorOr<GetLaborAttendanceRequestDto>>
    {
        private readonly ILaborAttendanceService _service;
        public UpdateLaborAttendanceCommandHandler(ILaborAttendanceService service) => _service = service;
        public Task<ErrorOr<GetLaborAttendanceRequestDto>> Handle(UpdateLaborAttendanceCommand request, CancellationToken cancellationToken)
            => _service.UpdateAsync(request.Dto);
    }
}
