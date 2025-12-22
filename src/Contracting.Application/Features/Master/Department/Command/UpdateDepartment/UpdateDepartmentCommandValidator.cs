using FluentValidation;

namespace Contracting.Application.Features.Master.Department.Command.UpdateDepartment
{
    public class UpdateDepartmentCommandValidator : AbstractValidator<UpdateDepartmentCommand>
    {
        public UpdateDepartmentCommandValidator()
        {
            RuleFor(x => x.Department.Id).NotEmpty();
            RuleFor(x => x.Department.nameEn).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Department.nameAr).NotEmpty().MaximumLength(100);
        }
    }
}
