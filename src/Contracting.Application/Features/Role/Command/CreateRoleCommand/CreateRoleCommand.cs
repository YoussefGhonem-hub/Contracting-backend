using Contracting.Shared.MasterDtos.RoleDto;
using MediatR;

namespace Contracting.Application.Features.Role.Command.CreateRoleCommand
{
    public class CreateRoleCommand : IRequest<Unit> // Specify Unit as the return type
    {
        public CreateRoleDto Dto { get; }

        public CreateRoleCommand(CreateRoleDto dto)
        {
            Dto = dto;
        }
    }
}