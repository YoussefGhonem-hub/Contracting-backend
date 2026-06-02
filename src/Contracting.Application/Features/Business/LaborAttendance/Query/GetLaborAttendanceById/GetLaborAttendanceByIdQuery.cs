using Contracting.Shared.BusinessDtos.LaborAttendanceDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Query.GetLaborAttendanceById
{
    public record GetLaborAttendanceByIdQuery(Guid Id) : IRequest<ErrorOr<GetLaborAttendanceRequestDto>>;
}
