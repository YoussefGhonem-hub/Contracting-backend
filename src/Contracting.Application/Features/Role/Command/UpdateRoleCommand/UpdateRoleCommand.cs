using Contracting.Shared.Common;
using Contracting.Shared.MasterDtos.RoleDto;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Command.UpdateRoleCommand
{
    public class UpdateRoleCommand : IRequest<ErrorOr<GenericResponse>>
    {
        public UpdateRoleDto Dto { get; }

        public UpdateRoleCommand(UpdateRoleDto dto)
        {
            Dto = dto;
        }
    }
}