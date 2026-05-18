using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.UpdateLaborAttendance
{
    public record UpdateLaborAttendanceCommand(UpdateLaborAttendanceRequestDto Dto) : IRequest<ErrorOr<GetLaborAttendanceRequestDto>>;
}
