using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface.client;
using Contracting.Shared.Dtos.ClientDtos.ClientManagementDtos;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Contracting.Application.Features.Client.ClientManagement.Command.UpdateClient
{
    public class UpdateClientCommandHandler : IRequestHandler<UpdateClientCommand, ErrorOr<GetClientDto>>
    {
        private readonly IClientService _service;
        private readonly UserManager<ApplicationUser> _userManager;

        public UpdateClientCommandHandler(IClientService service, UserManager<ApplicationUser> userManager)
        {
            _service = service;
            _userManager = userManager;
        }

        public async Task<ErrorOr<GetClientDto>> Handle(UpdateClientCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Client;

            var user = await _userManager.FindByIdAsync(dto.ApplicationUserId.ToString());
            if (user is null)
                return Error.NotFound("Client.UserNotFound", "Client user not found.");

            // Guard against changing the email to one already used by a different user.
            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var other = await _userManager.FindByEmailAsync(dto.Email);
                if (other is not null && other.Id != user.Id)
                    return Error.Conflict("Client.EmailExists", "A user with this email already exists.");

                user.Email = dto.Email;
                user.UserName = dto.Email;
            }

            user.FullName = dto.FullName;
            user.PhoneNumber = dto.PhoneNumber;

            var updateUserResult = await _userManager.UpdateAsync(user);
            if (!updateUserResult.Succeeded)
                return Error.Validation("Client.UpdateUserFailed",
                    updateUserResult.Errors.Select(e => e.Description).FirstOrDefault() ?? "Could not update client user.");

            var client = await _service.UpdateClientAsync(dto);
            if (client is null)
                return Error.NotFound("Client.NotFound", "Client not found.");

            return client;
        }
    }
}
