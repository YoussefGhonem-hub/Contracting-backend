using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.DeleteLaborAttendance
{
    public class DeleteLaborAttendanceCommandHandler : IRequestHandler<DeleteLaborAttendanceCommand, ErrorOr<GenericResponse>>
    {
        private readonly ILaborAttendanceService _service;
        public DeleteLaborAttendanceCommandHandler(ILaborAttendanceService service) => _service = service;
        public Task<ErrorOr<GenericResponse>> Handle(DeleteLaborAttendanceCommand request, CancellationToken cancellationToken)
            => _service.DeleteAsync(request.Id);
    }
}
