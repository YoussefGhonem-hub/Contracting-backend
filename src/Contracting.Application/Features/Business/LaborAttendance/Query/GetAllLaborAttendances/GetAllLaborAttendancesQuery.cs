using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Query.GetAllLaborAttendances
{
    public record GetAllLaborAttendancesQuery(LaborAttendanceFilterDto Filter) : IRequest<PaginatedList<GetLaborAttendanceRequestDto>>;
}
