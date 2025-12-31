using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Priority.Query.GetAllPriority
{
    public class GetAllPriorityQueryValidator : AbstractValidator<GetAllPriorityQuery>
    {
        public GetAllPriorityQueryValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Filter.PageIndex)
                .GreaterThan(0).WithMessage(localizer[SharedResourcesKeys.PageIndexMustBeGreaterThanZero]);

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage(localizer[SharedResourcesKeys.PageSizeMustBeGreaterThanZero])
                .LessThanOrEqualTo(100).WithMessage(localizer[SharedResourcesKeys.PageSizeMustBeLessThan100]);
        }
    }
}
