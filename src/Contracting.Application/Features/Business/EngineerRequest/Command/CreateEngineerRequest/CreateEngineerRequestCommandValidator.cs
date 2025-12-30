using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.EngineerRequest.Command.CreateEngineerRequest
{
    public class CreateEngineerRequestCommandValidator : AbstractValidator<CreateEngineerRequestCommand>
    {
        public CreateEngineerRequestCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
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
                .MaximumLength(10000).WithMessage(string.Format(localizer[SharedResourcesKeys.DescriptionMaxLength], 10000));
        }
    }
}