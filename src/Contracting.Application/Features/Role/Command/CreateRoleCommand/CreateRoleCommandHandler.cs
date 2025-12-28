using Contracting.Infrustructure.Inteface;
using MediatR;

namespace Contracting.Application.Features.Role.Command.CreateRoleCommand
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, Unit>
    {
        private readonly IRoleService _roleService;

        public CreateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<Unit> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            await _roleService.CreateRoleAsync(request.Dto);
            return Unit.Value;
        }
    }
}