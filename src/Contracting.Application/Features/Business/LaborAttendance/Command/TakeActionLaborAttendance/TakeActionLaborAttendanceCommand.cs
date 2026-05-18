using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.TakeActionLaborAttendance
{
    public record TakeActionLaborAttendanceCommand(Guid Id, LaborAttendanceActionDto ActionDto) : IRequest<ErrorOr<GetLaborAttendanceRequestDto>>;
}
