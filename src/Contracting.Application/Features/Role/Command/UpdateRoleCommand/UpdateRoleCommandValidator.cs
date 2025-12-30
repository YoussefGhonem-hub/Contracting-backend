using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Role.Command.UpdateRoleCommand
{
    public class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.RoleNameRequired]);
            
            RuleFor(x => x.Dto.DisplayName)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.RoleDisplayNameRequired]);
        }
    }
}