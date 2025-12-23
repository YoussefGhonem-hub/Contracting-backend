using FluentValidation;

namespace Contracting.Application.Features.Master.Priority.Command.DeletePriority
{
    public class DeletePriorityCommandValidator : AbstractValidator<DeletePriorityCommand>
    {
        public DeletePriorityCommandValidator()
        {
            RuleFor(x => x.PriorityId)
                .NotEmpty().WithMessage("Priority ID is required.");
        }
    }
}