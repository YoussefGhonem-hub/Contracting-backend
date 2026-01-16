using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Constants;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer
{
    public class UpdateEngineerCommandHandler : IRequestHandler<UpdateEngineerCommand, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IStringLocalizer<SharedResources> _localizer;

        public UpdateEngineerCommandHandler(IEngineerService service, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(UpdateEngineerCommand request, CancellationToken cancellationToken)
        {
            // Only when changing department and engineer has Teamlead role, ensure department has no existing manager
            if (request.Engineer.ChangeDepartmentId.HasValue && request.Engineer.ChangeDepartmentId != Guid.Empty)
            {
                var teamleadRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Teamleadengineer, cancellationToken);

                if (teamleadRole != null && request.Engineer.Roles != null && request.Engineer.Roles.Contains(teamleadRole.Id))
                {
                    var hasManager = await _service.CheckDepartmentHaveManagerAsync(request.Engineer.ChangeDepartmentId.Value);
                    if (hasManager)
                        return Error.Validation("Department.ManagerExists", "The selected department already has a manager.");
                }
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
