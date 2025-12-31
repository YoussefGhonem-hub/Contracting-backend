using Contracting.Infrustructure.Extensions.Helpers;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Query.GetAllRole
{
    public class GetAllRoleQueryHandler : IRequestHandler<GetAllRoleQuery, ErrorOr<PaginatedList<RoleDropDownDto>>>
    {
        private readonly IRoleService _service;

        public GetAllRoleQueryHandler(IRoleService service)
        {
            _service = service;
        }

        public async Task<ErrorOr<PaginatedList<RoleDropDownDto>>> Handle(GetAllRoleQuery request, CancellationToken cancellationToken)
        {
            var result = await _service.GetAllRolesAsync(request.Filter, cancellationToken);
            return result;
        }
    }
}
