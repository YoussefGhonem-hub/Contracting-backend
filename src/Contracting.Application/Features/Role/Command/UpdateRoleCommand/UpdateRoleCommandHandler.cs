using Contracting.Infrustructure.Inteface;
using MediatR;

namespace Contracting.Application.Features.Role.Command.UpdateRoleCommand
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, Unit>
    {
        private readonly IRoleService _roleService;

        public UpdateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Unit> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            await _roleService.UpdateRoleAsync(request.Dto);
            return Unit.Value;
        }
    }
}