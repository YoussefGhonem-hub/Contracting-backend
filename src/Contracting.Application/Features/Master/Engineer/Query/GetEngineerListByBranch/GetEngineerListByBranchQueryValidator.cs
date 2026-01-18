using FluentValidation;

namespace Contracting.Application.Features.Master.Engineer.Query.GetEngineerListByBranch
{
    public class GetEngineerListByBranchQueryValidator : AbstractValidator<GetEngineerListByBranchQuery>
    {
        public GetEngineerListByBranchQueryValidator()
        {
            RuleFor(x => x.Filter.PageIndex).GreaterThan(0);
            RuleFor(x => x.Filter.PageSize).GreaterThan(0);
        }
    }
}
