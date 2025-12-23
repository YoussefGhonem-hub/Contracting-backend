using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestById
{
    public class GetRequestByIdQueryValidator : AbstractValidator<GetRequestByIdQuery>
    {
        public GetRequestByIdQueryValidator()
        {
            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage("Request ID is required.");
        }
    }
}