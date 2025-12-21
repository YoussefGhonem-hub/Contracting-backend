using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Contracting.Application.Features.Master.Branch.Command.CreateBranch
{
    public class CreateBranchCommandValidator : AbstractValidator<CreateBranchCommand>
    {
        public CreateBranchCommandValidator()
        {
            RuleFor(x => x.Branch.nameEn)
                .NotEmpty().WithMessage("English name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Branch.nameAr)
                .NotEmpty().WithMessage("Arabic name is required.")
                .MaximumLength(100);

            RuleFor(x => x.Branch.address).NotEmpty();
            RuleFor(x => x.Branch.location).NotEmpty();
        }
    }

}
