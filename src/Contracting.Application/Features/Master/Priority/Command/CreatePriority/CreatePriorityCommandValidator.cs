using FluentValidation;

namespace Contracting.Application.Features.Master.Priority.Command.CreatePriority
{
    public class CreatePriorityCommandValidator : AbstractValidator<CreatePriorityCommand>
    {
        public CreatePriorityCommandValidator()
        {
            RuleFor(x => x.Priority.nameEn)
                .NotEmpty().WithMessage("Priority name (English) is required.")
                .MaximumLength(100).WithMessage("Priority name (English) cannot exceed 100 characters.");

            RuleFor(x => x.Priority.nameAr)
                .NotEmpty().WithMessage("Priority name (Arabic) is required.")
                .MaximumLength(100).WithMessage("Priority name (Arabic) cannot exceed 100 characters.");

            RuleFor(x => x.Priority.code)
                .NotEmpty().WithMessage("Priority code is required.")
                .MaximumLength(50).WithMessage("Priority code cannot exceed 50 characters.");
        }
    }
}