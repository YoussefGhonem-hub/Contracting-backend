using Contracting.Shared.MasterDtos.RoleDto;
using MediatR;

namespace Contracting.Application.Features.Role.Command.UpdateRoleCommand
{
    public class UpdateRoleCommand : IRequest<Unit> // Specify Unit as the return type
    {
        public UpdateRoleDto Dto { get; }

        public UpdateRoleCommand(UpdateRoleDto dto)
        {
            Dto = dto;
        }
    }
}