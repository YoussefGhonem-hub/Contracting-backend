using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Department.Command.UpdateDepartment
{
    public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Department.Id)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);
            
            RuleFor(x => x.Department.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));
            
            RuleFor(x => x.Department.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));
        }
    }
}
