using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.DeleteEngineerRequest
{
    public class DeleteEngineerRequestCommandValidator : AbstractValidator<DeleteEngineerRequestCommand>
    {
        public DeleteEngineerRequestCommandValidator()
        {
            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("Request ID is required.");
        }
    }
}