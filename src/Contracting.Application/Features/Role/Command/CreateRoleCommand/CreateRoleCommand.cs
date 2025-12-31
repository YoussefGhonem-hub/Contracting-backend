using Contracting.Shared.Common;
using Contracting.Shared.Dtos.MasterDtos.RoleDtos;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Command.CreateRoleCommand
{
    public class CreateRoleCommand : IRequest<ErrorOr<GenericResponse>>
    {
        public CreateRoleDto Dto { get; }

        public CreateRoleCommand(CreateRoleDto dto)
        {
            Dto = dto;
        }
    }
}