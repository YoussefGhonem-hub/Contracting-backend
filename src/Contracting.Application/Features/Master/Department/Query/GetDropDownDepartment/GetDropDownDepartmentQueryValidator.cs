using FluentValidation;

namespace Contracting.Application.Features.Master.Department.Query.GetDropDownDepartment
{
    public class GetDropDownDepartmentQueryValidator : AbstractValidator<GetDropDownDepartmentQuery>
    {
        public GetDropDownDepartmentQueryValidator()
        {
            RuleFor(x => x.BranchId).NotEmpty();           
        }
    }
}
