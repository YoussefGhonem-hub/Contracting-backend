using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Command.DeleteRoleCommand
{
    public class DeleteRoleCommand : IRequest<ErrorOr<GenericResponse>>
    {
        public Guid RoleId { get; }

        public DeleteRoleCommand(Guid roleId)
        {
            RoleId = roleId;
        }
    }
}