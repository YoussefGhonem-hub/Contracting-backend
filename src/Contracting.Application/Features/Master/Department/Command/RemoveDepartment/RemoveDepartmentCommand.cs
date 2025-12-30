using Contracting.Shared.Common;
using ErrorOr;
using MediatR;
using System;

namespace Contracting.Application.Features.Master.Department.Command.RemoveDepartment
{
    public record RemoveDepartmentCommand(Guid BranchId, Guid DepartmentId)
        : IRequest<ErrorOr<GenericResponse>>;
}
