using Contracting.Infrustructure.Inteface;
using MediatR;

namespace Contracting.Application.Features.Role.Command.DeleteRoleCommand
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, Unit>
    {
        private readonly IRoleService _roleService;

        public DeleteRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Unit> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            await _roleService.DeleteRoleAsync(request.RoleId);
            return Unit.Value;
        }
    }
}