using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;
using System;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;

namespace Contracting.Application.Features.Master.Department.Query.GetDropDownDepartment
{
    public record GetDropDownDepartmentQuery(Guid BranchId)
        : IRequest<ErrorOr<List<GetDepartmentDto>>>;
}
