using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.ReassignLaborAttendance
{
    public record ReassignLaborAttendanceCommand(Guid Id, ReassignLaborAttendanceDto Dto) : IRequest<ErrorOr<GetLaborAttendanceRequestDto>>;
}
