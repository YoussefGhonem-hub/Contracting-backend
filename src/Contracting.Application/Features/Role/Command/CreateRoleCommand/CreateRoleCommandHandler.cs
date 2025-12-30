using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Common;
using ErrorOr;
using MediatR;

namespace Contracting.Application.Features.Role.Command.CreateRoleCommand
{
    public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ErrorOr<GenericResponse>>
    {
        private readonly IRoleService _roleService;

        public CreateRoleCommandHandler(IRoleService roleService)
        {
            _roleService = roleService;
        }

        public async Task<ErrorOr<GenericResponse>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        {
            var result = await _roleService.CreateRoleAsync(request.Dto);
            return result.Success
                ? result
                : Error.Failure(result.Message ?? "Failed to create role");
        }
    }
}