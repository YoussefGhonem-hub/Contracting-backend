using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using MediatR;

public class GetRolesDropdownQueryHandler : IRequestHandler<GetRolesDropdownQuery, List<RoleDropDownDto>>
{
    private readonly IRoleService _roleService;

    public GetRolesDropdownQueryHandler(IRoleService roleService)
    {
        _roleService = roleService;
    }

    public async Task<List<RoleDropDownDto>> Handle(GetRolesDropdownQuery request, CancellationToken cancellationToken)
    {
        return await _roleService.GetRolesDropdownAsync();
    }
}
