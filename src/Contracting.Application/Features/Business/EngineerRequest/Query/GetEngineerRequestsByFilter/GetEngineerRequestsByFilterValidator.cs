using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetEngineerRequestsByFilter
{
    public class GetEngineerRequestsByFilterValidator : AbstractValidator<GetEngineerRequestsByFilterQuery>
    {
        public GetEngineerRequestsByFilterValidator()
        {
            RuleFor(x => x.Filter.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex must be greater than 0.");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");

            RuleFor(x => x)
                .Must(x => !x.Filter.FromDate.HasValue || !x.Filter.ToDate.HasValue || x.Filter.FromDate <= x.Filter.ToDate)
                .WithMessage("FromDate must be earlier than or equal to ToDate.");
        }
    }
}
