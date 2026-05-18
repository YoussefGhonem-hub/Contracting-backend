using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Query.GetLaborAttendanceById
{
    public class GetLaborAttendanceByIdQueryHandler : IRequestHandler<GetLaborAttendanceByIdQuery, ErrorOr<GetLaborAttendanceRequestDto>>
    {
        private readonly ILaborAttendanceService _service;
        public GetLaborAttendanceByIdQueryHandler(ILaborAttendanceService service) => _service = service;
        public Task<ErrorOr<GetLaborAttendanceRequestDto>> Handle(GetLaborAttendanceByIdQuery request, CancellationToken cancellationToken)
            => _service.GetByIdAsync(request.Id);
    }
}
