using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Project.Command.UpdateProject
{
    public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
    {
        public UpdateProjectCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Project.Id)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);

            RuleFor(x => x.Project.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(200).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 200));

            RuleFor(x => x.Project.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(200).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 200));

            RuleFor(x => x.Project.location)
                .MaximumLength(500).WithMessage(string.Format(localizer[SharedResourcesKeys.LocationMaxLength], 500));

            RuleFor(x => x.Project.BranchId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage(localizer[SharedResourcesKeys.InvalidGuid]);
        }
    }
}