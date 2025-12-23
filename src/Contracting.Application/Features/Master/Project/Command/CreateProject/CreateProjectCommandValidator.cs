using FluentValidation;

namespace Contracting.Application.Features.Master.Project.Command.CreateProject
{
    public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
    {
        public CreateProjectCommandValidator()
        {
            RuleFor(x => x.Project.nameEn)
                .NotEmpty().WithMessage("Project name (English) is required.")
                .MaximumLength(200).WithMessage("Project name (English) cannot exceed 200 characters.");

            RuleFor(x => x.Project.nameAr)
                .NotEmpty().WithMessage("Project name (Arabic) is required.")
                .MaximumLength(200).WithMessage("Project name (Arabic) cannot exceed 200 characters.");

            RuleFor(x => x.Project.location)
                .MaximumLength(500).WithMessage("Location cannot exceed 500 characters.");

            RuleFor(x => x.Project.BranchId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("BranchId must be either null or a valid GUID.");
        }
    }
}