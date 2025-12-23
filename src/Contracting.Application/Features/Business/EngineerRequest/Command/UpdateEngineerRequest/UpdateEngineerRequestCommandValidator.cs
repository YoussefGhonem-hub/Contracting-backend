using FluentValidation;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest
{
    public class UpdateEngineerRequestCommandValidator : AbstractValidator<UpdateEngineerRequestCommand>
    {
        public UpdateEngineerRequestCommandValidator()
        {
            RuleFor(x => x.Request.Id)
                .NotEmpty().WithMessage("Request ID is required.");

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
                .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
        }
    }
}