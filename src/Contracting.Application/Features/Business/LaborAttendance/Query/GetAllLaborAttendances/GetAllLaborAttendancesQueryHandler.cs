using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface.business;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Query.GetAllLaborAttendances
{
    public class GetAllLaborAttendancesQueryHandler : IRequestHandler<GetAllLaborAttendancesQuery, PaginatedList<GetLaborAttendanceRequestDto>>
    {
        private readonly ILaborAttendanceService _service;
        public GetAllLaborAttendancesQueryHandler(ILaborAttendanceService service) => _service = service;
        public Task<PaginatedList<GetLaborAttendanceRequestDto>> Handle(GetAllLaborAttendancesQuery request, CancellationToken cancellationToken)
            => _service.GetAllAsync(request.Filter);
    }
}
