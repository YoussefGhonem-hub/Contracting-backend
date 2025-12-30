using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer
{
    public class UpdateEngineerCommandValidator : AbstractValidator<UpdateEngineerCommand>
    {
        public UpdateEngineerCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Engineer.Id)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);
            
            RuleFor(x => x.Engineer.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));
            
            RuleFor(x => x.Engineer.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));
            
            RuleFor(x => x.Engineer.ApplicationUserId)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);
        }
    }
}
