using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Command.DeleteEngineerDepartment
{
    public record DeleteEngineerDepartmentCommand(Guid EngineerId, Guid DepartmentId) : IRequest<ErrorOr<GenericResponse>>;
}
