using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Status.Command.CreateStatus
{
    public class CreateStatusCommandValidator : AbstractValidator<CreateStatusCommand>
    {
        public CreateStatusCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Status.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));

            RuleFor(x => x.Status.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));

            RuleFor(x => x.Status.Code)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.CodeRequired])
                .MaximumLength(50).WithMessage(string.Format(localizer[SharedResourcesKeys.CodeMaxLength], 50));

            RuleFor(x => x.Status.orderNumber).GreaterThan(0);
        }
    }
}