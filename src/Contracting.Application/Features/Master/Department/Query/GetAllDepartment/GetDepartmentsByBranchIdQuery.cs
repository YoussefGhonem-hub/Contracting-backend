using Contracting.Shared.MasterDtos.DepartmentDtos;
using Contracting.Infrustructure.Extensions.Helpers;
using ErrorOr;
using MediatR;
using System;
using Contracting.Shared.Dtos;

namespace Contracting.Application.Features.Master.Department.Query.GetDepartmentsByBranchId
{
    public record GetDepartmentsByBranchIdQuery(Guid BranchId, BaseFilterDto Filter)
        : IRequest<ErrorOr<PaginatedList<GetDepartmentDto>>>;
}
