using Contracting.Application.Resources;
using Contracting.Domain.Entities;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer
{
    public class UpdateEngineerCommandHandler : IRequestHandler<UpdateEngineerCommand, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UpdateEngineerCommandHandler(IEngineerService service, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(UpdateEngineerCommand request, CancellationToken cancellationToken)
        {
            // Validate if the department already has a manager
            if (request.Engineer.ChangeDepartmentId != null || request.Engineer.ChangeDepartmentId == Guid.Empty)
            {
                var hasManager = await _service.CheckDepartmentHaveManagerAsync(request.Engineer.ChangeDepartmentId ?? Guid.Empty);
                if (hasManager)
                    return Error.Validation("Department.ManagerExists", "The selected department already has a manager.");
            }

            // Update the user details
            var userUpdateResult = await _service.UpdateUserAsync(request.Engineer.ApplicationUserId, request.Engineer);
            if (userUpdateResult is null)
                return Error.NotFound("Engineer not found.");

            // Update the engineer details
            var engineerUpdateResult = await _service.UpdateEngineerAsync(request.Engineer);
            if (engineerUpdateResult is null)
                return Error.NotFound("Engineer not found.");

            // Update the user roles
            await _service.UpdateUserRolesAsync(request.Engineer.ApplicationUserId, request.Engineer.Roles);

            return engineerUpdateResult;
        }
    }
}
