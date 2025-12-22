using FluentValidation;

namespace Contracting.Application.Features.Master.Engineer.Command.CreateEngineer
{
    public class CreateEngineerCommandValidator : AbstractValidator<CreateEngineerCommand>
    {
        public CreateEngineerCommandValidator()
        {
            RuleFor(x => x.Engineer.nameEn).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Engineer.nameAr).NotEmpty().MaximumLength(100);
            // Add more rules as needed
        }
    }
}
