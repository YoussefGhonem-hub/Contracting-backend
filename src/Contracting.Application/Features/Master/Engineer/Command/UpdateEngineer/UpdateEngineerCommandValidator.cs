using FluentValidation;

namespace Contracting.Application.Features.Master.Engineer.Command.UpdateEngineer
{
    public class UpdateEngineerCommandValidator : AbstractValidator<UpdateEngineerCommand>
    {
        public UpdateEngineerCommandValidator()
        {
            RuleFor(x => x.Engineer.Id).NotEmpty();
            RuleFor(x => x.Engineer.nameEn).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Engineer.nameAr).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Engineer.ApplicationUserId).NotEmpty();
            // Add more rules as needed
        }
    }
}
