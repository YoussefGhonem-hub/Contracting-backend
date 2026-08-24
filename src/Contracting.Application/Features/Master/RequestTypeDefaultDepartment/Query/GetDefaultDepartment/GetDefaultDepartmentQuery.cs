using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Query.GetDefaultDepartment
{
    public record GetDefaultDepartmentQuery(Guid BranchId, string RequestType) : IRequest<ErrorOr<GetDefaultDepartmentDto>>;
}
