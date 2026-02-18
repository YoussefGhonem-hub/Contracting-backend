using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.TakeActionOnRequest
{
    public class TakeActionRequestCommandValidator : AbstractValidator<TakeActionRequestCommand>
    {
        public TakeActionRequestCommandValidator()
        {
            RuleFor(x => x.RequestId).NotEmpty();
        }
    }
}