using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Priority.Command.UpdatePriority
{
    public class UpdatePriorityCommandValidator : AbstractValidator<UpdatePriorityCommand>
    {
        public UpdatePriorityCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Priority.Id)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);

            RuleFor(x => x.Priority.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));

            RuleFor(x => x.Priority.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));

            RuleFor(x => x.Priority.code)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.CodeRequired])
                .MaximumLength(50).WithMessage(string.Format(localizer[SharedResourcesKeys.CodeMaxLength], 50));
        }
    }
}