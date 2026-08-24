using Contracting.Shared.Dtos.MasterDtos.RequestTypeDefaultDepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Query.GetAllRequestTypeDefaultDepartments
{
    public record GetAllRequestTypeDefaultDepartmentsQuery(Guid BranchId) : IRequest<ErrorOr<List<GetRequestTypeDefaultDepartmentDto>>>;
}
