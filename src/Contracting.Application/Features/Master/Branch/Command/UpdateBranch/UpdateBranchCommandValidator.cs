using Contracting.Application.Features.Master.Branch.Command.UpdateBranch;
using FluentValidation;

public class UpdateBranchCommandValidator : AbstractValidator<UpdateBranchCommand>
{
    public UpdateBranchCommandValidator()
    {
        RuleFor(x => x.Branch.Id).NotEmpty();
        RuleFor(x => x.Branch.nameEn).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Branch.nameAr).NotEmpty().MaximumLength(100);
    }
}
