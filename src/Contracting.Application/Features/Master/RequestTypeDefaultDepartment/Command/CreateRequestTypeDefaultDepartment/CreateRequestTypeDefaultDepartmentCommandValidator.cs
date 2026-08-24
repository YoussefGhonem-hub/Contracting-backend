using Contracting.Shared.Constants;
using FluentValidation;

namespace Contracting.Application.Features.Master.RequestTypeDefaultDepartment.Command.CreateRequestTypeDefaultDepartment
{
    public class CreateRequestTypeDefaultDepartmentCommandValidator : AbstractValidator<CreateRequestTypeDefaultDepartmentCommand>
    {
        public CreateRequestTypeDefaultDepartmentCommandValidator()
        {
            RuleFor(x => x.Dto.BranchId)
                .NotEmpty().WithMessage("BranchId is required.");

            RuleFor(x => x.Dto.DepartmentId)
                .NotEmpty().WithMessage("DepartmentId is required.");

            RuleFor(x => x.Dto.RequestType)
                .NotEmpty().WithMessage("RequestType is required.")
                .Must(rt => RequestTypeKeys.All.Contains(rt))
                .WithMessage($"RequestType must be one of: {string.Join(", ", RequestTypeKeys.All)}.");
        }
    }
}
