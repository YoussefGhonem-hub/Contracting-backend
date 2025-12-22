using FluentValidation;

namespace Contracting.Application.Features.Master.Department.Query.GetDepartmentsByBranchId
{
    public class GetDepartmentsByBranchIdQueryValidator : AbstractValidator<GetDepartmentsByBranchIdQuery>
    {
        public GetDepartmentsByBranchIdQueryValidator()
        {
            RuleFor(x => x.BranchId).NotEmpty();
            RuleFor(x => x.Filter.PageIndex).GreaterThan(0);
            RuleFor(x => x.Filter.PageSize).GreaterThan(0);
        }
    }
}
