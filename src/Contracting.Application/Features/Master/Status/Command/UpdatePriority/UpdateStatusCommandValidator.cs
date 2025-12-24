using FluentValidation;

namespace Contracting.Application.Features.Master.Status.Command.UpdateStatus
{
    public class UpdateStatusCommandValidator : AbstractValidator<UpdateStatusCommand>
    {
        public UpdateStatusCommandValidator()
        {
            RuleFor(x => x.Status.Id)
                .NotEmpty().WithMessage("Status ID is required.");

            RuleFor(x => x.Status.nameEn)
                .NotEmpty().WithMessage("Status name (English) is required.")
                .MaximumLength(100).WithMessage("Status name (English) cannot exceed 100 characters.");

            RuleFor(x => x.Status.nameAr)
                .NotEmpty().WithMessage("Status name (Arabic) is required.")
                .MaximumLength(100).WithMessage("Status name (Arabic) cannot exceed 100 characters.");

            RuleFor(x => x.Status.Code)
                .NotEmpty().WithMessage("Status code is required.")
                .MaximumLength(50).WithMessage("Status code cannot exceed 50 characters.");
            RuleFor(x => x.Status.orderNumber).Empty().WithMessage("Status Order Number Is Required").GreaterThan(0).WithMessage("Status Order Number Should be Greater Then 0");

        }
    }
}