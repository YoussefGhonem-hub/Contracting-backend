using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Master.Status.Query.GetAllStatus
{
    public class GetAllStatusQueryValidator : AbstractValidator<GetAllStatusQuery>
    {
        public GetAllStatusQueryValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.Filter.PageIndex)
                .GreaterThan(0).WithMessage(localizer[SharedResourcesKeys.PageIndexMustBeGreaterThanZero]);

            RuleFor(x => x.Filter.PageSize)
                .GreaterThan(0).WithMessage(localizer[SharedResourcesKeys.PageSizeMustBeGreaterThanZero])
                .LessThanOrEqualTo(100).WithMessage(localizer[SharedResourcesKeys.PageSizeMustBeLessThan100]);
        }
    }
}
