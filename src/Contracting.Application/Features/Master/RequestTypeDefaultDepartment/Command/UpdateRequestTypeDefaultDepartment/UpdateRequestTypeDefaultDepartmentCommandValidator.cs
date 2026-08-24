using FluentValidation;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.UpdateRequestTypeDefaultDepartment
{
    public class UpdateRequestTypeDefaultDepartmentCommandValidator : AbstractValidator<UpdateRequestTypeDefaultDepartmentCommand>
    {
        public UpdateRequestTypeDefaultDepartmentCommandValidator()
        {
            RuleFor(x => x.Dto.Id).NotEmpty().WithMessage("Id is required.");
            RuleFor(x => x.Dto.DepartmentId).NotEmpty().WithMessage("DepartmentId is required.");
        }
    }
}
