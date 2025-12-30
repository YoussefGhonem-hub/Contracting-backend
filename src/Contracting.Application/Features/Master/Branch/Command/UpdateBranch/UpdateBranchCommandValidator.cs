using Contracting.Application.Features.Master.Branch.Command.UpdateBranch;
using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator(IStringLocalizer<SharedResources> localizer)
    {
        RuleFor(x => x.Branch.Id)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);
        
        RuleFor(x => x.Branch.nameEn)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
            .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));
        
        RuleFor(x => x.Branch.nameAr)
            .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
            .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));
    }
}
