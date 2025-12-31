using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Shared.Dtos;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Query.GetAllRole
{
    public record GetAllRoleQuery(BaseFilterDto Filter)
        : IRequest<ErrorOr<PaginatedList<RoleDropDownDto>>>;
}
