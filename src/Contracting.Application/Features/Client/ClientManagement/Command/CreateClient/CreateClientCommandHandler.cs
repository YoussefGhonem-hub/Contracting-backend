using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Constants;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Application.Features.Client.ClientManagement.Command.CreateClient
{
    public class CreateClientCommandHandler : IRequestHandler<CreateClientCommand, ErrorOr<GetClientDto>>
    {
        private readonly IClientService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateClientCommandHandler(IClientService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public async Task<ErrorOr<GetClientDto>> Handle(CreateClientCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Client;

            // Reject duplicate email up front for a clean message.
            var existing = await _userManager.FindByEmailAsync(dto.Email!);
            if (existing is not null)
                return Error.Conflict("Client.EmailExists", "A user with this email already exists.");

            // Create the login user.
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                IsActive = true
            };

            var createResult = await _userManager.CreateAsync(user, dto.Password ?? string.Empty);
            if (!createResult.Succeeded)
                return Error.Validation("Client.CreateUserFailed",
                    createResult.Errors.Select(e => e.Description).FirstOrDefault() ?? "Could not create client user.");

            // Assign the Client role.
            var roleResult = await _userManager.AddToRoleAsync(user, RoleNames.Client);
            if (!roleResult.Succeeded)
            {
                // Roll back the orphaned user so we don't leave a roleless account behind.
                await _userManager.DeleteAsync(user);
                return Error.Failure("Client.AssignRoleFailed",
                    roleResult.Errors.Select(e => e.Description).FirstOrDefault() ?? "Could not assign the Client role.");
            }

            // Create the client profile (and project links).
            var client = await _service.CreateClientAsync(dto, user.Id);
            return client;
        }
    }
}
