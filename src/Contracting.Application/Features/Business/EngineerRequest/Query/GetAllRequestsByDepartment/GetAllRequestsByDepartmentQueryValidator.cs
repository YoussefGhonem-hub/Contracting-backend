using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Query.GetAllRequestsByDepartment
{
    public class GetAllRequestsByDepartmentQueryValidator : AbstractValidator<GetAllRequestsByDepartmentQuery>
    {
        public GetAllRequestsByDepartmentQueryValidator()
        {
            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Department ID is required.");

            RuleFor(x => x.Filter.PageIndex)
                .GreaterThan(0).WithMessage("PageIndex must be greater than 0.");

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0.")
                .LessThanOrEqualTo(100).WithMessage("PageSize cannot exceed 100.");
        }
    }
}