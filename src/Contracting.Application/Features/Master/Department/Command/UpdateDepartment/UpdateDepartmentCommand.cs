using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;
using System;

namespace Contracting.Application.Features.Master.Department.Command.UpdateDepartment
{
    public record UpdateDepartmentCommand(UpdateDepartmentDto Department)
        : IRequest<ErrorOr<GetDepartmentDto>>;
}
