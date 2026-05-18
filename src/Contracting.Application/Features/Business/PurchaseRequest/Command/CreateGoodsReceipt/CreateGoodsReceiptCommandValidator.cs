using Contracting.Shared.Resources;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace Contracting.Application.Features.Business.PurchaseRequest.Command.CreateGoodsReceipt
{
    public class CreateGoodsReceiptCommandValidator : AbstractValidator<CreateGoodsReceiptCommand>
    {
        public CreateGoodsReceiptCommandValidator(IStringLocalizer<SharedResources> localizer)
        {
            RuleFor(x => x.RequestId)
                .NotEmpty().WithMessage(localizer[SharedResourcesKeys.InvalidGuid]);
        }
    }
}
