using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Contracting.Application.Features.Users.Commands.UploadUserSignature;

public class UploadUserSignatureCommandValidator : AbstractValidator<UploadUserSignatureCommand>
{
    private const long MaxSignatureSizeBytes = 2 * 1024 * 1024; // 2 MB

    public UploadUserSignatureCommandValidator()
    {
        RuleFor(x => x.Signature)
            .NotNull().WithMessage("Signature file is required.");

        RuleFor(x => x.Signature)
            .Must(file => file is null || file.Length > 0)
            .WithMessage("Signature file cannot be empty.");

        RuleFor(x => x.Signature)
            .Must(BeImage)
            .WithMessage("Signature must be an image.")
            .When(x => x.Signature is not null);

        RuleFor(x => x.Signature)
            .Must(file => file is null || file.Length <= MaxSignatureSizeBytes)
            .WithMessage("Signature must be 2 MB or smaller.");
    }

    private static bool BeImage(IFormFile? file)
    {
        if (file is null) return true;
        if (string.IsNullOrWhiteSpace(file.ContentType)) return false;
        return file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
    }
}
