using FluentValidation;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerList
{
    public class GetEngineerListQueryValidator : AbstractValidator<GetEngineerListQuery>
    {
        public GetEngineerListQueryValidator()
        {
            RuleFor(x => x.Filter.PageIndex).GreaterThan(0);
            RuleFor(x => x.Filter.PageSize).GreaterThan(0);
        }
    }
}
