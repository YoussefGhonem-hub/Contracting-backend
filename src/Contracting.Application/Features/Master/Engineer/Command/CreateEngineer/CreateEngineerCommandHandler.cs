using Contracting.Shared.Resources;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Contracting.Shared.Dtos.MasterDtos.EngineerDto;

namespace Contracting.Application.Features.Master.Engineer.Command.CreateEngineer
{
    public class CreateEngineerCommandHandler : IRequestHandler<CreateEngineerCommand, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IStringLocalizer<SharedResources> _localizer;


        public CreateEngineerCommandHandler(IEngineerService service, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _userManager = userManager;
            _roleManager = roleManager;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(CreateEngineerCommand request, CancellationToken cancellationToken)
        {
            // Get the Teamlead-engineer role ID by looking up the role name
            var teamleadRole = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Name == RoleNames.Teamleadengineer);

            if (teamleadRole != null && request.Engineer.Roles.Contains(teamleadRole.Id))
            {
                var hasManager = await _service.CheckDepartmentHaveManagerAsync(request.Engineer.DepartmentId);
                if (hasManager) 
                    return Error.Validation("Department.ManagerExists", "The selected department already has a manager.");
            }

            // Check TeamLead uniqueness per department in DepartmentRoles
            if (teamleadRole != null && request.Engineer.DepartmentRoles != null)
            {
                foreach (var dr in request.Engineer.DepartmentRoles.Where(d => d.RoleId == teamleadRole.Id))
                {
                    var hasManager = await _service.CheckDepartmentHaveManagerAsync(dr.DepartmentId);
                    if (hasManager)
                        return Error.Validation("Department.ManagerExists", $"Department already has a TeamLead.");
                }
            }

            // You can add any additional logic here if needed, such as checking user permissions
            // Create User
            var user = new ApplicationUser
            {
                UserName = request.Engineer.Email,
                Email = request.Engineer.Email,
                FullName = request.Engineer.nameEn,
                PhoneNumber = request.Engineer.phoneNumber,
                IsActive = true
            };
            var Result = await _userManager.CreateAsync(user, request.Engineer.password ?? string.Empty);
            if (!Result.Succeeded)
                return Error.Validation("General.Validation", Result.Errors.Select(e => e.Description).FirstOrDefault() ?? _localizer[SharedResourcesKeys.InvalidCredentials]);

            // Update roles using the service method
            await _service.UpdateUserRolesAsync(user.Id, request.Engineer.Roles);

            // Create Engineer
            var engineer = await _service.CreateEngineerAsync(request.Engineer, user.Id);
            if (engineer is null)
                return Error.Failure("Could not create engineer.");

            // Save department-role assignments
            if (request.Engineer.DepartmentRoles != null && request.Engineer.DepartmentRoles.Any())
            {
                await _service.ReplaceEngineerDepartmentsAsync(engineer.Id, request.Engineer.DepartmentRoles);
            }

            return engineer;
        }
    }
}
