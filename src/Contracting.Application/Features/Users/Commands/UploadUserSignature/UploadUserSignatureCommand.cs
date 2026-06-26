using Contracting.Domain.Entities.helper;
using Contracting.Infrustructure.Persistence;
using Contracting.Shared.CurrentUser;
using Contracting.Shared.Dtos.HelperDtos;
using Contracting.Shared.Storage;
using ErrorOr;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Storage.AWS3.Services;

namespace Contracting.Application.Features.Users.Commands.UploadUserSignature;

public record UploadUserSignatureCommand(IFormFile? Signature) : IRequest<ErrorOr<UserSignatureDto>>;

public class UploadUserSignatureCommandHandler : IRequestHandler<UploadUserSignatureCommand, ErrorOr<UserSignatureDto>>
{
    private readonly ApplicationDbContext _db;
    private readonly IFileStorage _storage;
    private readonly IStorageService _s3;

    public UploadUserSignatureCommandHandler(ApplicationDbContext db, IFileStorage storage, IStorageService s3)
    {
        _db = db;
        _storage = storage;
        _s3 = s3;
    }

    public async Task<ErrorOr<UserSignatureDto>> Handle(UploadUserSignatureCommand request, CancellationToken cancellationToken)
    {
        var currentUserId = CurrentUser.Id;
        if (!currentUserId.HasValue)
            return Error.Unauthorized("Signature.Unauthorized", "User is not authenticated.");

        var file = request.Signature;
        if (file is null || file.Length == 0)
            return Error.Validation("Signature.FileRequired", "Signature file is required.");

        var signature = await _db.UserSignatures
            .FirstOrDefaultAsync(x => x.UserId == currentUserId.Value, cancellationToken);

        var contentType = string.IsNullOrWhiteSpace(file.ContentType)
            ? "application/octet-stream"
            : file.ContentType;

        string? previousPath = signature?.SignatureUrl;
        await using var readStream = file.OpenReadStream();
        var newPath = await _storage.SaveUserSignatureAsync(
            currentUserId.Value,
            readStream,
            file.FileName,
            contentType,
            cancellationToken);

        if (signature is null)
        {
            signature = new UserSignature
            {
                UserId = currentUserId.Value,
                SignatureUrl = newPath,
                FileName = file.FileName,
                ContentType = contentType,
                FileSize = file.Length
            };

            _db.UserSignatures.Add(signature);
        }
        else
        {
            signature.SignatureUrl = newPath;
            signature.FileName = file.FileName;
            signature.ContentType = contentType;
            signature.FileSize = file.Length;
        }

        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(previousPath) && !string.Equals(previousPath, newPath, StringComparison.OrdinalIgnoreCase))
        {
            await _storage.DeleteAsync(previousPath, cancellationToken);
        }

        return MapToDto(signature, _s3);
    }

    private static UserSignatureDto MapToDto(UserSignature entity, IStorageService s3) => new UserSignatureDto
    {
        Id = entity.Id,
        UserId = entity.UserId,
        SignatureUrl = string.IsNullOrWhiteSpace(entity.SignatureUrl)
            ? entity.SignatureUrl
            : s3.GetPreSignedUrl(entity.SignatureUrl),
        FileName = entity.FileName,
        ContentType = entity.ContentType,
        FileSize = entity.FileSize,
        CreatedDate = entity.CreatedDate,
        ModifiedDate = entity.ModifiedDate
    };
}
