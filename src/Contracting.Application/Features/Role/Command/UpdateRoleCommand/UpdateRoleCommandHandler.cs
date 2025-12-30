using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Command.UpdateRoleCommand
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ErrorOr<GenericResponse>>
    {
        private readonly IRoleService _roleService;

        public UpdateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _roleService.UpdateRoleAsync(request.Dto);
            return result.Success
                ? result
                : Error.Failure(result.Message ?? "Failed to update role");
        }
    }
}