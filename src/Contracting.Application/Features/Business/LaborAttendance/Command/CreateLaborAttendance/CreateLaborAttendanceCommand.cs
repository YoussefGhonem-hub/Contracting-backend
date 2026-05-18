using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.CreateLaborAttendance
{
    public record CreateLaborAttendanceCommand(CreateLaborAttendanceRequestDto Dto) : IRequest<ErrorOr<GetLaborAttendanceRequestDto>>;
}
