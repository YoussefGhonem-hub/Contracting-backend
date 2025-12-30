using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.CreateBranch
{
    public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
    {
        public CreateBranchCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Branch.nameEn)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameEnRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameEnMaxLength], 100));

            RuleFor(x => x.Branch.nameAr)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.NameArRequired])
                .MaximumLength(100).WithMessage(string.Format(localizer[SharedResourcesKeys.NameArMaxLength], 100));

            RuleFor(x => x.Branch.address)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.AddressRequired]);
            
            RuleFor(x => x.Branch.location)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.LocationRequired]);
        }
    }

}
