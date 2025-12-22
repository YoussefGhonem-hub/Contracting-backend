using Contracting.Shared.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;
using System;

namespace Contracting.Application.Features.Master.Department.Command.CreateDepartment
{
    public record CreateDepartmentCommand(Guid BranchId, CreateDepartmentDto Department)
        : IRequest<ErrorOr<GetDepartmentDto>>;
}
