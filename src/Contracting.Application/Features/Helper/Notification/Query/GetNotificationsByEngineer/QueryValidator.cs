using FluentValidation;

namespace Contracting.Application.Features.Helper.Notification.Query.GetNotificationsByEngineer
{
    public class GetNotificationsByEngineerQueryValidator : AbstractValidator<GetNotificationsByEngineerQuery>
    {
        public GetNotificationsByEngineerQueryValidator()
        {
            RuleFor(x => x.EngineerId).NotEmpty();
            RuleFor(x => x.Filter.PageIndex).GreaterThan(0);
            RuleFor(x => x.Filter.PageSize).GreaterThan(0);
        }
    }
}
