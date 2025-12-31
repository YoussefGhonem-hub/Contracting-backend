using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.UpdateEngineerRequest
{
    public class UpdateEngineerRequestCommandValidator : AbstractValidator<UpdateEngineerRequestCommand>
    {
        public UpdateEngineerRequestCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Request.Id)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.Required]);

            RuleFor(x => x.Request.ProjectId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage(localizer[SharedResourcesKeys.InvalidGuid]);

            RuleFor(x => x.Request.DepartmentId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage(localizer[SharedResourcesKeys.InvalidGuid]);

            RuleFor(x => x.Request.PriorityId)
                .Must(id => id == null || id != Guid.Empty)
                .WithMessage(localizer[SharedResourcesKeys.InvalidGuid]);

            RuleFor(x => x.Request.Descreption)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.DescriptionRequired])
                .MaximumLength(1000).WithMessage(string.Format(localizer[SharedResourcesKeys.DescriptionMaxLength], 1000));
        }
    }
}