using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetRequestsByStatusForEngineer
{
    public class GetRequestsByStatusForEngineerValidator : AbstractValidator<GetRequestsByStatusForEngineerQuery>
    {
        public GetRequestsByStatusForEngineerValidator()
        {
            RuleFor(x => x.Filter.StatusId)
                .NotEmpty().WithMessage("StatusId is required.");

            RuleFor(x => x.Filter.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex must be greater than 0.");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");
        }
    }
}
