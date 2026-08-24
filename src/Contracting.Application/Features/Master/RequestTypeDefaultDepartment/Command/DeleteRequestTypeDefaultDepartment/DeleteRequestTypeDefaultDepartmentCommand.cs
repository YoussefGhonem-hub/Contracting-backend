using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.DeleteRequestTypeDefaultDepartment
{
    public record DeleteRequestTypeDefaultDepartmentCommand(Guid Id) : IRequest<ErrorOr<GenericResponse>>;
}
