using FluentValidation;

namespace Contracting.Application.Features.Master.Status.Command.DeleteStatus
{
    public class DeleteStatusCommandValidator : AbstractValidator<DeleteStatusCommand>
    {
        public DeleteStatusCommandValidator()
        {
            RuleFor(x => x.StatusId)
                .NotEmpty().WithMessage("Status ID is required.");
        }
    }
}