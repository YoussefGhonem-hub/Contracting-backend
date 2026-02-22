using Contracting.Shared.Dtos.MasterDtos.DepartmentDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Department.Query.GetDepartmentSpecialFields
{
    public record GetDepartmentSpecialFieldsQuery(Guid DepartmentId) : IRequest<ErrorOr<DepartmentSpecialFieldsCheckDto>>;
}
