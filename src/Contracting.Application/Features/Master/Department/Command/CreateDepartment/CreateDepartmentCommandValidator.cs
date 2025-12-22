using FluentValidation;

namespace Contracting.Application.Features.Master.Department.Command.CreateDepartment
{
    public class CreateDepartmentCommandValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentCommandValidator()
        {
            RuleFor(x => x.BranchId).NotEmpty();
            RuleFor(x => x.Department.nameEn).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Department.nameAr).NotEmpty().MaximumLength(100);
        }
    }
}
