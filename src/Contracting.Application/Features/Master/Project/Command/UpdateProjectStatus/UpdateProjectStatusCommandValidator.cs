using Contracting.Shared.Common.Enums;
using FluentValidation;

namespace Contracting.Application.Features.Master.Project.Command.UpdateProjectStatus;

public class UpdateProjectStatusCommandValidator : AbstractValidator<UpdateProjectStatusCommand>
{
    public UpdateProjectStatusCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required.");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid project status value.");
    }
}
