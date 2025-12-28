using MediatR;

namespace Contracting.Application.Features.Role.Command.DeleteRoleCommand
{
    public class DeleteRoleCommand : IRequest<Unit> // Specify Unit as the return type
    {
        public Guid RoleId { get; }

        public DeleteRoleCommand(Guid roleId)
        {
            RoleId = roleId;
        }
    }
}