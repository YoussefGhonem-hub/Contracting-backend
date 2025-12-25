using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest
{
    public class CreateEngineerRequestCommandValidator : AbstractValidator<CreateEngineerRequestCommand>
    {
        public CreateEngineerRequestCommandValidator()
        {
            RuleFor(x => x.Request.ProjectId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("ProjectId must be either null or a valid GUID.");

            RuleFor(x => x.Request.DepartmentId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("DepartmentId must be either null or a valid GUID.");

            RuleFor(x => x.Request.PriorityId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("PriorityId must be either null or a valid GUID.");

            RuleFor(x => x.Request.EngineerId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage("EngineerId must be either null or a valid GUID.");

            RuleFor(x => x.Request.Descreption)
                .NotEmpty().WithMessage("Description is required.")
                .MaximumLength(10000).WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}