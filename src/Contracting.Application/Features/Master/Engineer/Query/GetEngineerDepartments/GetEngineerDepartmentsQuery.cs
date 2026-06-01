using Contracting.Shared.Dtos.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerDepartments
{
    public record GetEngineerDepartmentsQuery(Guid EngineerId) : IRequest<ErrorOr<List<EngineerDepartmentRoleDto>>>;
}
