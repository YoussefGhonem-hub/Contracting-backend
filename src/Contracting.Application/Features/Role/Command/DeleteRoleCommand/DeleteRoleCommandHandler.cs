using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Command.DeleteRoleCommand
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, ErrorOr<GenericResponse>>
    {
        private readonly IRoleService _roleService;

        public DeleteRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _roleService.DeleteRoleAsync(request.RoleId);
            return result.Success
                ? result
                : Error.Failure(result.Message ?? "Failed to delete role");
        }
    }
}