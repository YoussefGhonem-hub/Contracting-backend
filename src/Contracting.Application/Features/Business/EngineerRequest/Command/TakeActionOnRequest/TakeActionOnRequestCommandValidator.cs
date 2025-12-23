using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public class TakeActionOnRequestCommandValidator : AbstractValidator<TakeActionOnRequestCommand>
    {
        public TakeActionOnRequestCommandValidator()
        {
            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("Request ID is required.");

            RuleFor(x => x.EngineerId)
                .NotEmpty().WithMessage("Engineer ID is required.");

            RuleFor(x => x.ActionNote)
                .MaximumLength(500).WithMessage("Action note cannot exceed 500 characters.");
        }
    }
}