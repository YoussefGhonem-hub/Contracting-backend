using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Contracting.Application.Features.Users.Commands.UpdateAccountSettings;

public sealed class UpdateAccountSettingsCommandValidator : AbstractValidator<UpdateAccountSettingsCommand>
{
    public UpdateAccountSettingsCommandValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name cannot exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.")
            .MaximumLength(256).WithMessage("Email cannot exceed 256 characters.");

        // Avatar is optional but if present must be an image and limited size (e.g., 5 MB)
        RuleFor(x => x.Avatar)
            .Must(BeImageOrNull).WithMessage("Avatar must be an image.")
            .Must(BeUnderSizeLimit).WithMessage("Avatar exceeds the maximum allowed size (5 MB).");
    }

    private static bool BeImageOrNull(IFormFile? file) =>
        file is null || (file.ContentType?.StartsWith("image/", System.StringComparison.OrdinalIgnoreCase) ?? false);

    private static bool BeUnderSizeLimit(IFormFile? file) =>
        file is null || file.Length <= 5 * 1024 * 1024;
}