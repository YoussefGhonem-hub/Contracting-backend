using Contracting.Application.Resources;
using Contracting.Domain.Entities;
using Contracting.Domain.Entities.master;
using Contracting.Infrustructure.Inteface;
using Contracting.Shared.Constants;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.MasterDtos.EngineerDto;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Engineer.Command.CreateEngineer
{
    public class CreateEngineerCommandHandler : IRequestHandler<CreateEngineerCommand, ErrorOr<GetEngineerDto>>
    {
        private readonly IEngineerService _service;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStringLocalizer<SharedResources> _localizer;


        public CreateEngineerCommandHandler(IEngineerService service, UserManager<ApplicationUser> userManager, IStringLocalizer<SharedResources> localizer)
        {
            _service = service;
            _userManager = userManager;
            _localizer = localizer;
        }

        public async Task<ErrorOr<GetEngineerDto>> Handle(CreateEngineerCommand request, CancellationToken cancellationToken)
        {
            if (request.Engineer.Roles.Contains(RoleNames.Teamleadengineer, StringComparer.OrdinalIgnoreCase))
            {
                var hasManager = await _service.CheckDepartmentHaveManagerAsync(request.Engineer.DepartmentId);
                if (hasManager) 
                    return Error.Validation("Department.ManagerExists", "The selected department already has a manager.");
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
            var Result = await _userManager.CreateAsync(user, request.Engineer.password);
            if (!Result.Succeeded)
                return Error.Validation("General.Validation", Result.Errors.Select(e => e.Description).FirstOrDefault() ?? _localizer[SharedResourcesKeys.InvalidCredentials]);

            // Update roles using the service method
            await _service.UpdateUserRolesAsync(user.Id, request.Engineer.Roles);

            // Create Engineer
            var engineer = await _service.CreateEngineerAsync(request.Engineer, user.Id);
            return engineer is null
                ? Error.Failure("Could not create engineer.")
                : engineer;
        }
    }
}
