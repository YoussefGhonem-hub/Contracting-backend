using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Business.LaborAttendance.Command.DeleteLaborAttendance
{
    public record DeleteLaborAttendanceCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
}
