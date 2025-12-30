using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Engineer.Command.CreateEngineer
{
    public class CreateEngineerCommandValidator : AbstractValidator<CreateEngineerCommand>
    {
        public CreateEngineerCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Engineer.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));
            
            RuleFor(x => x.Engineer.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));
            
            RuleFor(x => x.Engineer.password)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
                .MaximumLength(100).WithMessage(localizer[SharedResourcesKeys.PasswordLength]);
            
            RuleFor(x => x.Engineer.Email)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required])
                .EmailAddress().WithMessage(localizer[SharedResourcesKeys.EmailInvalid]);
        }
    }
}
