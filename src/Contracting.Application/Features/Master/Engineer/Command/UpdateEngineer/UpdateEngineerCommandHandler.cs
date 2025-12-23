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
            if ((request.Engineer.ChangeDepartmentId != null || request.Engineer.ChangeDepartmentId == Guid.Empty) && request.Engineer.isManager)
            {
                var hasManager = await _service.CheckDepartmentHaveManagerAsync(request.Engineer.ChangeDepartmentId ?? new Guid());
                if (hasManager)
                    return Error.Validation("Department.ManagerExists", "The selected department already has a manager.");
            }

            // ? FIXED: Fetch the existing user from the database
            var user = await _userManager.FindByIdAsync(request.Engineer.ApplicationUserId.ToString());
            if (user is null)
                return Error.NotFound("User not found.");

            // ? Update only the properties that can change
            user.UserName = request.Engineer.Email;
            user.Email = request.Engineer.Email;
            user.FullName = request.Engineer.nameEn;
            user.PhoneNumber = request.Engineer.phoneNumber;
            user.IsActive = true;

            // ? Now update the user
            var Result = await _userManager.UpdateAsync(user);
            if (!Result.Succeeded)
                return Error.Validation("General.Validation", Result.Errors.Select(e => e.Description).FirstOrDefault() ?? _localizer[SharedResourcesKeys.InvalidCredentials]);

            // Update the engineer
            var result = await _service.UpdateEngineerAsync(request.Engineer);
            return result is null
                ? Error.NotFound("Engineer not found.")
                : result;
        }
    }
}
